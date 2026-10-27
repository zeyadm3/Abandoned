"""Organic bodies for the remade monsters: a node graph (joints with radii) becomes one continuous sculpt.

skin_body(): vertices with skin radii + the Skin modifier + subdivision give a seamless zbrush-style base;
voxel remesh closes every junction; sculpt.* fields add anatomy (ribs, tendons, wrinkles, cracks). The result is
the HIGH bake source; the game mesh is a decimated copy (sculpt.retopo-like) carrying the detail in its maps.

Frame: as the 2026-10-08 threats (Tools/Blender/threats.py) so prefabs, AI and takes keep their orientation:
facing -Y, +Z up, feet on Z = 0; the rig is rig.threat_rig() from the same proportions.
"""
import math

import bmesh
import bpy
from mathutils import Vector

from ..core import kit


class Body:
    """Collects nodes (position, radius) and edges, then skins them into one mesh."""

    def __init__(self):
        self.nodes, self.edges, self.bones = [], [], []
        self.bone = 'spine'  # the bone new nodes belong to (weights follow the nearest nodes' bones)

    def node(self, p, r, bone=None):
        self.nodes.append((Vector(p), r))
        self.bones.append(bone or self.bone)
        return len(self.nodes) - 1

    def chain(self, points, radii, start=None, bone=None):
        """A run of nodes joined in order; optionally attached to an existing node index."""
        ids = []
        prev = start
        for p, r in zip(points, radii):
            i = self.node(p, r, bone)
            if prev is not None:
                self.edges.append((prev, i))
            ids.append(i)
            prev = i
        return ids

    def limb(self, a, b, ra, rb, segments=4, start=None, bend=(0, 0, 0), bone=None):
        """Nodes from a to b with radius ra -> rb; bend pushes the middle sideways (muscle bellies, sag)."""
        a, b, bend = Vector(a), Vector(b), Vector(bend)
        pts, rs = [], []
        for k in range(segments + 1):
            t = k / segments
            pts.append(a.lerp(b, t) + bend * math.sin(math.pi * t))
            # A muscle belly a third of the way down, tapering to the joint.
            rs.append((ra + (rb - ra) * t) * (1 + .22 * math.sin(math.pi * min(1, t * 1.4))))
        if start is not None:
            pts, rs = pts[1:], rs[1:]
        return self.chain(pts, rs, start, bone)

    def build(self, name, subdiv=2, voxel=.012, inflate=1.6):
        """inflate: the skin modifier plus subdivision and smoothing shrink a graph's radii by about a third."""
        bm = bmesh.new()
        verts = [bm.verts.new(p) for p, _ in self.nodes]
        for a, b in self.edges:
            bm.edges.new((verts[a], verts[b]))
        ob = kit.from_bmesh(name, bm)
        sk = ob.modifiers.new('Skin', 'SKIN')
        sk.use_smooth_shade = True
        sk.branch_smoothing = 1.0
        layer = ob.data.skin_vertices[0].data
        for i, (_, r) in enumerate(self.nodes):
            rr = tuple(r[:2]) if isinstance(r, (tuple, list)) else (r, r)
            layer[i].radius = (rr[0] * inflate, rr[1] * inflate)
        # Every separate piece needs its own root, or the Skin modifier drops it.
        parent = list(range(len(self.nodes)))

        def find(i):
            while parent[i] != i:
                parent[i] = parent[parent[i]]
                i = parent[i]
            return i
        for a, b in self.edges:
            parent[find(a)] = find(b)
        seen = set()
        for i in range(len(self.nodes)):
            r = find(i)
            if r not in seen:
                seen.add(r)
                layer[i].use_root = True
        sub = ob.modifiers.new('Sub', 'SUBSURF')
        sub.levels = sub.render_levels = subdiv
        kit.apply(ob)
        rem = ob.modifiers.new('Remesh', 'REMESH')
        # Below about 1.2 cm Blender's voxel remesh hits its cell limit and returns almost nothing.
        rem.mode, rem.voxel_size, rem.use_smooth_shade = 'VOXEL', max(voxel, .012), True
        kit.apply(ob)
        sm = ob.modifiers.new('Smooth', 'CORRECTIVE_SMOOTH')
        sm.iterations, sm.factor, sm.smooth_type = 6, .5, 'LENGTH_WEIGHTED'
        kit.apply(ob)
        ob.data.shade_smooth()
        return ob


def blob(name, centre, radii, subdiv=3):
    """A soft ellipsoid (sacks, tumours, bellies) to union with a body."""
    ob = kit.sphere(name, 1.0, centre, 32, 16, scale=radii)
    return ob


def union(base, others, voxel=.012):
    """Merge extra shapes into the body as one surface (boolean union, then remesh)."""
    for o in others:
        kit.boolean(base, o, 'UNION')
    rem = base.modifiers.new('Remesh', 'REMESH')
    rem.mode, rem.voxel_size, rem.use_smooth_shade = 'VOXEL', max(voxel, .012), True
    kit.apply(base)
    return base


def humanoid(height, width, leg, arm, body_y=0.0, *, torso_r=.16, hip_r=.13, neck_r=.06, head=(.1, .12, .13),
             upper_arm=.06, fore_arm=.045, thigh=.08, shin=.055, hand=.05, fingers=4, finger_len=.12, finger_r=.012,
             hunch=0.0, foot_len=.22, splay=0.0):
    """A humanoid node graph on the threat rig's landmarks (threats.py rig_begin): shoulders at 0.76h, hips at
    `leg`, head from 0.89h to h. Returns (Body, landmarks dict)."""
    b = Body()
    y = body_y
    sh_z, neck_z, head_z = height * .76, height * .89, height
    hips = b.node((0, y, leg), hip_r, 'spine')
    belly = b.node((0, y - hunch * .2, leg + (sh_z - leg) * .45), torso_r * .95)
    b.edges.append((hips, belly))
    chest = b.node((0, y - hunch * .6, sh_z - .05), torso_r)
    b.edges.append((belly, chest))
    neck = b.node((0, y - hunch, neck_z), neck_r, 'neck')
    b.edges.append((chest, neck))
    skull = b.node((0, y - hunch * 1.1, (neck_z + head_z) / 2 + .02), head, 'head')
    b.edges.append((neck, skull))
    if isinstance(head, (tuple, list)) and len(head) > 2 and head[2] > max(head[0], head[1]):
        # A tall skull: a second node above carries the height the skin modifier's two radii can't.
        crown = b.node(b.nodes[skull][0] + Vector((0, .01, head[2] * .5)), (head[0] * .85, head[1] * .85), 'head')
        b.edges.append((skull, crown))
    marks = {'hips': hips, 'chest': chest, 'neck': neck, 'skull': skull}
    for s, n in ((-1, 'L'), (1, 'R')):
        sh = (s * width, y - hunch * .6, sh_z)
        el = (s * (width + .04 + splay), y - .06, sh_z - arm * .5)
        wr = (s * (width + .08 + splay), y - .12, sh_z - arm)
        clav = b.node(((s * width * .5), y - hunch * .6, sh_z), torso_r * .55)
        b.edges.append((chest, clav))
        ua = b.limb(sh, el, upper_arm, fore_arm * 1.1, 3, bone='arm' + n)
        b.edges.append((clav, ua[0]))
        fa = b.limb(el, wr, fore_arm * 1.05, fore_arm * .8, 3, start=ua[-1], bone='forearm' + n)
        palm = b.node((s * (width + .09 + splay), y - .14, sh_z - arm - hand * .8), hand, 'forearm' + n)
        b.edges.append((fa[-1], palm))
        for f in range(fingers):
            spread = (f - (fingers - 1) / 2) * .025
            tip = Vector((s * (width + .1 + splay) + spread, y - .2, sh_z - arm - hand - finger_len))
            mid = Vector((s * (width + .095 + splay) + spread, y - .17, sh_z - arm - hand - finger_len * .5))
            b.chain([mid, tip], [finger_r, finger_r * .6], palm, 'forearm' + n)
        hip = (s * width * .43, y, leg)
        kn = (s * width * .46, y + .03, leg * .48)
        an = (s * width * .5, y + .02, .1)
        th = b.limb(hip, kn, thigh, shin * 1.1, 3, bone='leg' + n)
        b.edges.append((hips, th[0]))
        sn = b.limb(kn, an, shin, shin * .7, 3, start=th[-1], bone='shin' + n)
        toe = b.node((s * width * .52, y - foot_len, .05), shin * .55, 'shin' + n)
        b.edges.append((sn[-1], toe))
        marks['hand' + n] = palm
        marks['foot' + n] = toe
    return b, marks


def weight(ob, body, armature, falloff=.06, extra=()):
    """Skin weights from the node graph: each vertex blends the bones of its nearest nodes (surface distance), so
    fingers follow the hand even when they hang beside a thigh. `extra` adds (position, radius, bone) points for
    shapes that were unioned in (sacks, plates, coats)."""
    import numpy as np
    pts = [p for p, _ in body.nodes] + [Vector(p) for p, _, _ in extra]
    rad = [(r[0] if isinstance(r, (tuple, list)) else r) for _, r in body.nodes] + [r for _, r, _ in extra]
    labels = body.bones + [b for _, _, b in extra]
    P = np.array([tuple(p) for p in pts], np.float64)
    R = np.array(rad, np.float64)
    names = sorted(set(labels))
    lab = np.array([names.index(b) for b in labels])
    n = len(ob.data.vertices)
    co = np.empty(n * 3)
    ob.data.vertices.foreach_get('co', co)
    co = co.reshape(-1, 3)
    mw = np.array(ob.matrix_world)
    co = co @ mw[:3, :3].T + mw[:3, 3]
    for nm in names:
        if ob.vertex_groups.get(nm) is None:
            ob.vertex_groups.new(name=nm)
    W = np.zeros((n, len(names)))
    for start in range(0, n, 4096):
        c = co[start:start + 4096]
        d = np.linalg.norm(c[:, None, :] - P[None, :, :], axis=2) - R[None, :]
        best = d.min(axis=1, keepdims=True)
        w = 1.0 / (1.0 + ((d - best) / falloff) ** 2) ** 3
        w[(d - best) > falloff * 3] = 0
        for k in range(len(names)):
            W[start:start + len(c), k] = (w * (lab == k)[None, :]).max(axis=1)
    W /= np.maximum(W.sum(axis=1, keepdims=True), 1e-9)
    for k, nm in enumerate(names):
        vg = ob.vertex_groups[nm]
        idx = np.nonzero(W[:, k] > .02)[0]
        for i in idx:
            vg.add([int(i)], float(W[i, k]), 'REPLACE')
    mod = ob.modifiers.get('Armature') or ob.modifiers.new('Armature', 'ARMATURE')
    mod.object = armature
    ob.parent = armature
    return ob
