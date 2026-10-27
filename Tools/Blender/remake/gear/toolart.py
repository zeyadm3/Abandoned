"""A Blender twin of Scripts/Equipment/ToolArtBuilder: the same primitives with the same meaning (Unity coordinates,
sizes as diameters, unit primitives along +Y, Quaternion.Euler rotations), so every gear design ports line for line
from C#, then gains what code-built meshes couldn't have: real bevels, smooth turned profiles, knurling and baked,
worn surfaces. Geometry is generated in Unity space and converted to Blender (x, y, z) -> (x, z, y) at the end.
"""
import math

import bmesh
import bpy
from mathutils import Euler, Matrix, Quaternion, Vector

from ..core import kit, materials

SWAP = Matrix(((1, 0, 0, 0), (0, 0, 1, 0), (0, 1, 0, 0), (0, 0, 0, 1)))


def unity_euler(e):
    """Quaternion.Euler(x, y, z): Z first, then X, then Y (degrees)."""
    if not e:
        return Matrix.Identity(4)
    x, y, z = (math.radians(a) for a in e)
    return (Matrix.Rotation(y, 4, 'Y') @ Matrix.Rotation(x, 4, 'X') @ Matrix.Rotation(z, 4, 'Z'))


def from_to(a, b):
    """Quaternion.FromToRotation(a, b) as a matrix."""
    a, b = Vector(a).normalized(), Vector(b).normalized()
    return a.rotation_difference(b).to_matrix().to_4x4()


def look(forward):
    """Quaternion.LookRotation(forward) (up = +Y): local +Z points along forward."""
    f = Vector(forward).normalized()
    up = Vector((0, 1, 0)) if abs(f.y) < .999 else Vector((1, 0, 0))
    r = up.cross(f).normalized()
    u = f.cross(r)
    return Matrix((r, u, f)).transposed().to_4x4()


class ToolArt:
    """Collect parts per material (Unity space); finish() returns one Blender object in remake axes."""

    def __init__(self, paint):
        L = materials.library()
        self.Paint = materials.paint(f'GearPaint_{"%.2f_%.2f_%.2f" % tuple(paint)}', paint, wear=.55, grime=.4)
        self.Rubber = L['Rubber']
        self.Steel = L['Steel']
        self.Canvas = materials.fabric('GearCanvas', (.27, .26, .2), .9, 400)
        self.Leather = L['Leather']
        self.Glass = L['Glass']
        self.Screen = L['Screen']
        self.Tape = materials.surface('GearTape', (.78, .78, .74), .3, .3, var=.05, grime=.5)
        self.Wood = L['WoodRaw']
        self.Dark = L['Plastic']
        self.parts = []

    # ---------------------------------------------------------------------------- primitives (unit, Unity-local)

    def _add(self, bm, M, mat, bevel=0.0):
        bmesh.ops.transform(bm, matrix=SWAP @ M, verts=bm.verts)
        bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
        ob = kit.from_bmesh('Part', bm)
        materials.assign(ob, mat)
        if bevel:
            kit.bevel_mod(ob, bevel, 2)
        self.parts.append(ob)
        return ob

    def _box(self, size, bevel):
        bm = bmesh.new()
        bmesh.ops.create_cube(bm, size=1.0)
        bmesh.ops.scale(bm, vec=Vector(size), verts=bm.verts)
        b = min(bevel, min(size) * .3)
        if b > .0004:
            bmesh.ops.bevel(bm, geom=list(bm.edges), offset=b, segments=2, profile=.5, affect='EDGES', clamp_overlap=True)
        return bm

    def _cyl(self, size, sides=32, chamfer=.07):
        """A turned cylinder along +Y like ToolArtMeshes.TurnedCylinder: chamfered rims."""
        rx, h, rz = size[0] * .5, size[1] * .5, size[2] * .5
        c = min(chamfer * min(rx, rz) * 2, h * .5)
        bm = bmesh.new()
        rings = []
        for y, k in ((-h, 1 - c / max(rx, 1e-6)), (-h + c, 1), (h - c, 1), (h, 1 - c / max(rx, 1e-6))):
            ring = [bm.verts.new((math.cos(t) * rx * k, y, math.sin(t) * rz * k)) for t in
                    (i / sides * math.tau for i in range(sides))]
            rings.append(ring)
        for lo, hi in zip(rings, rings[1:]):
            for i in range(sides):
                j = (i + 1) % sides
                bm.faces.new((lo[i], lo[j], hi[j], hi[i]))
        bm.faces.new(list(reversed(rings[0])))
        bm.faces.new(rings[-1])
        for f in bm.faces:
            f.smooth = len(f.verts) == 4
        return bm

    def _washer(self, size, sides=32):
        ro, ri, h = size[0] * .5, size[0] * .34, size[1] * .5
        bm = bmesh.new()
        q = []
        for r, y in ((ri, -h), (ro, -h), (ro, h), (ri, h)):
            q.append([bm.verts.new((math.cos(t) * r, y, math.sin(t) * r)) for t in (i / sides * math.tau for i in range(sides))])
        for k in range(4):
            a, b = q[k], q[(k + 1) % 4]
            for i in range(sides):
                j = (i + 1) % sides
                bm.faces.new((a[i], a[j], b[j], b[i]))
        for f in bm.faces:
            f.smooth = True
        return bm

    # ---------------------------------------------------------------------------- the ToolArtBuilder vocabulary

    def Box(self, at, size, mat, euler=None):
        M = Matrix.Translation(at) @ unity_euler(euler)
        return self._add(self._box(size, .004), M, mat)

    def Cylinder(self, at, size, mat, euler=None):
        M = Matrix.Translation(at) @ unity_euler(euler)
        return self._add(self._cyl(size), M, mat)

    def Disc(self, at, diameter, depth, mat):
        return self.Cylinder(at, (diameter, depth, diameter), mat, (90, 0, 0))

    def Ring(self, at, diameter, thickness, mat, euler=None):
        M = Matrix.Translation(at) @ unity_euler(euler)
        return self._add(self._washer((diameter, thickness, diameter)), M, mat)

    def Rod(self, a, b, diameter, mat):
        a, b = Vector(a), Vector(b)
        M = Matrix.Translation((a + b) * .5) @ from_to((0, 1, 0), b - a)
        return self._add(self._cyl((diameter, (b - a).length, diameter), 24, .12), M, mat)

    def Beam(self, a, b, width, depth, mat):
        a, b = Vector(a), Vector(b)
        M = Matrix.Translation((a + b) * .5) @ look(b - a)
        return self._add(self._box((width, depth, (b - a).length), .003), M, mat)

    def Screws(self, centre, width, length, y, rows=2):
        for row in range(rows):
            for side in (-1, 1):
                at = Vector(centre) + Vector((side * width * .5, y, 0 if rows == 1 else (row / (rows - 1) - .5) * length))
                self.Cylinder(at, (.008, .003, .008), self.Steel)
                self.Box(at + Vector((0, .002, 0)), (.005, .001, .0015), self.Dark)

    def Grille(self, at, size, count, frame):
        self.Box(at, (size[0], .003, size[1]), self.Dark)
        for i in range(count):
            self.Box(Vector(at) + Vector((0, .002, (i / max(1, count - 1) - .5) * size[1] * .86)),
                     (size[0] * .88, .0025, .003), frame)

    def Grip(self, a, b, diameter):
        """A rubber grip with real knurled ridges (12 rings instead of 6, each a rounded torus)."""
        self.Rod(a, b, diameter, self.Rubber)
        a, b = Vector(a), Vector(b)
        for i in range(1, 13):
            at = a.lerp(b, i / 13)
            M = Matrix.Translation(at) @ from_to((0, 1, 0), b - a)
            self._add(self._washer((diameter * 1.05, .003, diameter * 1.05)), M, self.Dark)

    def Cable(self, a, b, sag, diameter, mat):
        a, b, sag = Vector(a), Vector(b), Vector(sag)
        pts = [a.lerp(b, i / 12) + sag * math.sin(i / 12 * math.pi) for i in range(13)]
        bpts = [SWAP @ p for p in pts]
        ob = kit.tube_along('Cable', bpts, diameter * .5, 10)
        materials.assign(ob, mat)
        self.parts.append(ob)

    def finish(self, name):
        for p in self.parts:
            kit.apply(p)
        return kit.join(self.parts, name)
