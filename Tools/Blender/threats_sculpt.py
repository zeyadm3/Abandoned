"""Original skin-bound sculpt primitives for the threat redesign; Blender-only authoring code."""
import math
import bpy
from mathutils import Vector


class ThreatSculpt:
    def __init__(self, materials, parts, bones):
        self.materials, self.parts, self.bones = materials, parts, bones

    def bind(self, obj, name, material, bone, smooth=True):
        obj.name = name
        obj.data.materials.append(self.materials[material])
        group = obj.vertex_groups.new(name=bone)
        group.add(list(range(len(obj.data.vertices))), 1.0, 'REPLACE')
        for face in obj.data.polygons:
            face.use_smooth = smooth
        self.parts.append(obj)
        return obj

    def mesh(self, name, vertices, faces, uv, material, bone, smooth=True):
        data = bpy.data.meshes.new(name + '_Surface')
        data.from_pydata(vertices, [], faces)
        data.update()
        layer = data.uv_layers.new(name='UVMap')
        for face in data.polygons:
            for index in face.loop_indices:
                layer.data[index].uv = uv[data.loops[index].vertex_index]
        obj = bpy.data.objects.new(name, data)
        bpy.context.collection.objects.link(obj)
        return self.bind(obj, name, material, bone, smooth)

    def ellipsoid(self, name, at, size, material, bone='spine', rotation=None):
        bpy.ops.mesh.primitive_uv_sphere_add(segments=18, ring_count=10, location=at)
        obj = bpy.context.object
        obj.scale = Vector(size) * .5
        if rotation:
            obj.rotation_euler = rotation
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        return self.bind(obj, name, material, bone)

    def box(self, name, at, size, material, bone='spine', rotation=None, bevel=.012):
        bpy.ops.mesh.primitive_cube_add(size=1, location=at)
        obj = bpy.context.object
        obj.scale = size
        if rotation:
            obj.rotation_euler = rotation
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        if bevel > 0:
            modifier = obj.modifiers.new('Rolled and worn edges', 'BEVEL')
            modifier.width = min(bevel, min(size) * .22)
            modifier.segments = 2
            bpy.ops.object.modifier_apply(modifier=modifier.name)
        return self.bind(obj, name, material, bone, smooth=False)

    def loft(self, name, rings, material, bone='spine', sides=18):
        """Elliptical sections (x, y, z, x radius, y radius), with UVs along the manufactured/organic surface."""
        vertices, uv, faces = [], [], []
        run = 0.0
        for row, (x, y, z, rx, ry) in enumerate(rings):
            if row:
                run += (Vector((x, y, z)) - Vector(rings[row - 1][:3])).length
            for side in range(sides + 1):
                angle = side * math.tau / sides
                vertices.append((x + math.cos(angle) * rx, y + math.sin(angle) * ry, z))
                uv.append((side / sides, run / .55))
                if row and side < sides:
                    a = (row - 1) * (sides + 1) + side
                    b = row * (sides + 1) + side
                    faces.append((a, a + 1, b + 1, b))
        faces.append(tuple(reversed(range(sides))))
        last = (len(rings) - 1) * (sides + 1)
        faces.append(tuple(last + i for i in range(sides)))
        return self.mesh(name, vertices, faces, uv, material, bone)

    def tube(self, name, points, material, bone='spine', sides=10):
        """A smoothly tapered, curved tube: each point is x/y/z/radius. End caps keep silhouettes closed."""
        vertices, uv, faces = [], [], []
        run = 0.0
        for row, point in enumerate(points):
            at = Vector(point[:3])
            before = Vector(points[max(0, row - 1)][:3])
            after = Vector(points[min(len(points) - 1, row + 1)][:3])
            tangent = (after - before).normalized()
            across = tangent.cross(Vector((0, 0, 1)))
            if across.length < .01:
                across = tangent.cross(Vector((0, 1, 0)))
            across.normalize()
            up = tangent.cross(across).normalized()
            if row:
                run += (at - before).length
            for side in range(sides + 1):
                angle = side * math.tau / sides
                vertices.append(at + (across * math.cos(angle) + up * math.sin(angle)) * point[3])
                uv.append((side / sides, run / .35))
                if row and side < sides:
                    a = (row - 1) * (sides + 1) + side
                    b = row * (sides + 1) + side
                    faces.append((a, a + 1, b + 1, b))
        faces.append(tuple(reversed(range(sides))))
        last = (len(points) - 1) * (sides + 1)
        faces.append(tuple(last + i for i in range(sides)))
        return self.mesh(name, vertices, faces, uv, material, bone)

    def line(self, name, start, end, radius, material, bone='spine', sides=8):
        a, b = Vector(start), Vector(end)
        return self.tube(name, [(*a, radius), (*b, radius)], material, bone, sides)

    def ring(self, name, at, rx, rz, thickness, material, bone='head', plane='XZ', segments=22):
        points = []
        for i in range(segments + 1):
            angle = i * math.tau / segments
            offset = (math.cos(angle) * rx, 0, math.sin(angle) * rz) if plane == 'XZ' else (math.cos(angle) * rx, math.sin(angle) * rz, 0)
            points.append((*tuple(Vector(at) + Vector(offset)), thickness))
        return self.tube(name, points, material, bone, sides=6)

    def plate(self, name, at, width, height, depth, material, bone='spine', tilt=0):
        # Clipped corners make forged plates and mask panels, rather than stacked unmodified boxes.
        outline = [(-.35, -.5), (.35, -.5), (.5, -.34), (.5, .34), (.34, .5), (-.34, .5), (-.5, .32), (-.5, -.32)]
        vertices, uv = [], []
        for y in [-depth * .5, depth * .5]:
            for x, z in outline:
                x, z = x * width, z * height
                vertices.append((at[0] + x * math.cos(tilt) - z * math.sin(tilt), at[1] + y,
                                 at[2] + x * math.sin(tilt) + z * math.cos(tilt)))
                uv.append((x / max(width, .01) + .5, z / max(height, .01) + .5))
        faces = [tuple(range(8)), tuple(reversed(range(8, 16)))]
        for i in range(8):
            j = (i + 1) % 8
            faces.append((i, i + 8, j + 8, j))
        return self.mesh(name, vertices, faces, uv, material, bone, smooth=False)

    def cloth(self, name, rows, material='Canvas', bone='spine', columns=10, folds=.025):
        """Rows are z/centre-x/centre-y/width. A thin, two-sided pleated strip with an uneven last edge."""
        vertices, uv, faces = [], [], []
        for row, (z, x, y, width) in enumerate(rows):
            for col in range(columns + 1):
                u = col / columns
                edge = (math.sin(col * 2.7) * .035 + math.sin(col * .9) * .02) if row == len(rows) - 1 else 0
                vertices.append((x + (u - .5) * width, y + math.sin(u * math.tau * 3) * folds, z + edge))
                uv.append((u, row / (len(rows) - 1)))
                if row and col < columns:
                    a = (row - 1) * (columns + 1) + col
                    b = row * (columns + 1) + col
                    faces.append((a, b, b + 1, a + 1))
        # Duplicate a thin back skin; these remain visible through torn openings from both sides.
        count = len(vertices)
        vertices += [(x, y + .012, z) for x, y, z in vertices]
        uv += list(uv)
        faces += [tuple(index + count for index in reversed(face)) for face in list(faces)]
        return self.mesh(name, vertices, faces, uv, material, bone)

    def anatomy(self, material, arm_width, leg_width, fingers=4, cuffs=False):
        for suffix, sign in [('L', -1), ('R', 1)]:
            for limb, width in [('arm', arm_width), ('forearm', arm_width * .74), ('leg', leg_width), ('shin', leg_width * .77)]:
                key = limb + suffix
                a, b = self.bones[key]
                bend = (a + b) * .5 + Vector((sign * .025, -.025, 0))
                self.tube(key + '_ShapedAnatomy', [(*a, width * .45), (*a.lerp(bend, .55), width * .56),
                    (*bend, width * .47), (*b, width * .31)], material, key, sides=14)
                self.ellipsoid(key + '_Joint', b, (width * .83, width * .88, width * .76), material, key)
            hand = self.bones['forearm' + suffix][1]
            self.ellipsoid('ArticulatedPalm', hand + Vector((0, -.015, -.055)),
                           (arm_width * 1.22, arm_width * .8, .20), material, 'forearm' + suffix)
            for digit in range(fingers):
                offset = (digit - (fingers - 1) * .5) * arm_width * .29
                start = hand + Vector((offset, -.025, -.11))
                middle = start + Vector((sign * .015, -.035, -.12 - .025 * math.sin(digit)))
                end = middle + Vector((sign * .01, -.07, -.075))
                self.tube('BentDigit', [(*start, arm_width * .12), (*middle, arm_width * .095), (*end, .008)],
                          material, 'forearm' + suffix, sides=7)
            self.tube('OpposedThumb', [(*(hand + Vector((sign * arm_width * .55, 0, -.015))), arm_width * .16),
                (*(hand + Vector((sign * arm_width * .7, -.06, -.11))), arm_width * .1),
                (*(hand + Vector((sign * arm_width * .4, -.10, -.14))), .009)], material, 'forearm' + suffix, sides=7)
            foot = self.bones['shin' + suffix][1]
            self.ellipsoid('WeightedFoot', foot + Vector((0, -.10, -.025)),
                           (leg_width * 1.1, .37, .17), material, 'shin' + suffix)
            if cuffs:
                for key in ['forearm' + suffix, 'shin' + suffix]:
                    a, b = self.bones[key]
                    at = a.lerp(b, .78)
                    self.ellipsoid('WornRestraintCuff', at, (arm_width * 1.02, arm_width * 1.05, .09), 'Iron', key)

    def stitches(self, name, a, b, material='Bone', bone='spine', count=12, width=.04):
        a, b = Vector(a), Vector(b)
        along = (b - a).normalized()
        cross = along.cross(Vector((0, -1, 0))).normalized() * width * .5
        for i in range(count):
            at = a.lerp(b, (i + .5) / count)
            self.line(name, at - cross, at + cross + along * width * .28, .0035, material, bone, sides=5)

    def hair_strand(self, name, points, start_radius=0.009, end_radius=0.002, material='Hair', bone='head', sides=6):
        """A single strand of wet, stringy, clumped horror hair with tapering radius."""
        pts = []
        for i, p in enumerate(points):
            t = i / max(1, len(points) - 1)
            r = start_radius * (1.0 - t * 0.78)
            pts.append((*p[:3], r))
        return self.tube(name, pts, material, bone, sides=sides)

    def hair_curtain(self, name, anchor, count, length, material='Hair', bone='head',
                     spread_x=0.18, spread_y=0.16, z_drop=0.05, forward_bias=-0.08, wave_amp=0.025):
        """Cascading clumps and strings of greasy, matted horror hair hanging from a cranial vault."""
        anchor = Vector(anchor)
        for i in range(count):
            u = (i / max(1, count - 1)) - 0.5
            angle = u * math.pi * 1.3
            ox = math.sin(angle) * spread_x + (math.sin(i * 3.7) * 0.02)
            oy = math.cos(angle) * spread_y * 0.7 + (math.cos(i * 2.3) * 0.03)
            oz = -abs(u) * z_drop
            start = anchor + Vector((ox, oy, oz))
            
            strand_len = length * (0.75 + (math.sin(i * 5.1) * 0.25))
            segments = 5
            pts = [start]
            cur = Vector(start)
            for s in range(1, segments + 1):
                t = s / segments
                grav = t * t * strand_len
                wav_x = math.sin(t * math.pi * 3.0 + i) * wave_amp
                wav_y = math.cos(t * math.pi * 2.5 + i * 1.5) * wave_amp * 0.5 + forward_bias * t
                cur = Vector((start.x + wav_x, start.y + wav_y, start.z - grav))
                pts.append(cur)
            self.hair_strand(f'{name}_{i}', pts, start_radius=0.008, end_radius=0.002, material=material, bone=bone, sides=5)

    def horror_maw(self, name, jaw_center, width, height, depth, teeth_count=14, bone='head',
                   gum_mat='Gore', tooth_mat='Bone', gaping=0.14):
        """A distended, unhinged horror jaw with exposed raw gums and needle-sharp bone fangs."""
        c = Vector(jaw_center)
        # Deep dark oral chasm
        self.ellipsoid(f'{name}_OralCavity', c + Vector((0, depth * 0.3, -height * 0.1)),
                       (width * 0.85, depth * 0.9, height * 0.85), 'Charcoal', bone)
        # Raw gory tissue arches (upper & lower gums)
        upper_gum = c + Vector((0, 0, height * 0.35))
        lower_gum = c + Vector((0, -gaping * 0.4, -height * 0.45 - gaping))
        self.tube(f'{name}_UpperGum', [
            (upper_gum.x - width * 0.5, upper_gum.y + depth * 0.2, upper_gum.z - 0.02, 0.022),
            (upper_gum.x - width * 0.25, upper_gum.y - depth * 0.2, upper_gum.z + 0.01, 0.026),
            (upper_gum.x, upper_gum.y - depth * 0.28, upper_gum.z + 0.015, 0.028),
            (upper_gum.x + width * 0.25, upper_gum.y - depth * 0.2, upper_gum.z + 0.01, 0.026),
            (upper_gum.x + width * 0.5, upper_gum.y + depth * 0.2, upper_gum.z - 0.02, 0.022)
        ], gum_mat, bone, sides=8)

        self.tube(f'{name}_LowerGum', [
            (lower_gum.x - width * 0.46, lower_gum.y + depth * 0.25, lower_gum.z + 0.02, 0.020),
            (lower_gum.x - width * 0.23, lower_gum.y - depth * 0.15, lower_gum.z - 0.01, 0.024),
            (lower_gum.x, lower_gum.y - depth * 0.22, lower_gum.z - 0.015, 0.025),
            (lower_gum.x + width * 0.23, lower_gum.y - depth * 0.15, lower_gum.z - 0.01, 0.024),
            (lower_gum.x + width * 0.46, lower_gum.y + depth * 0.25, lower_gum.z + 0.02, 0.020)
        ], gum_mat, bone, sides=8)

        # Upper needle teeth pointing down and inward
        n_up = teeth_count // 2
        for i in range(n_up):
            u = (i / max(1, n_up - 1)) - 0.5
            tx = u * width * 0.8
            ty = -depth * 0.26 + abs(u) * depth * 0.35
            tz = upper_gum.z
            t_len = 0.045 + (1.0 - abs(u) * 1.5) * 0.035
            if i % 3 == 0: t_len *= 1.35 # Wicked long fangs
            fang_tip = (c.x + tx, c.y + ty + 0.01, tz - t_len, 0.002)
            self.tube(f'{name}_UpperTooth_{i}', [
                (c.x + tx, c.y + ty, tz, 0.009),
                fang_tip
            ], tooth_mat, bone, sides=5)

        # Lower needle teeth pointing up and outward
        n_down = teeth_count - n_up
        for i in range(n_down):
            u = (i / max(1, n_down - 1)) - 0.5
            tx = u * width * 0.74
            ty = lower_gum.y + abs(u) * depth * 0.3
            tz = lower_gum.z
            t_len = 0.04 + (1.0 - abs(u) * 1.5) * 0.03
            if (i + 1) % 3 == 0: t_len *= 1.4 # Lower wicked fangs
            fang_tip = (c.x + tx, ty - 0.015, tz + t_len, 0.002)
            self.tube(f'{name}_LowerTooth_{i}', [
                (c.x + tx, ty, tz, 0.008),
                fang_tip
            ], tooth_mat, bone, sides=5)

        # Stretched necrotic muscle tendrils on jaw hinges
        for sign in [-1, 1]:
            hinge_top = upper_gum + Vector((sign * width * 0.48, depth * 0.15, 0))
            hinge_bot = lower_gum + Vector((sign * width * 0.44, depth * 0.18, 0))
            self.line(f'{name}_JawTendon_{sign}', hinge_top, hinge_bot, 0.013, gum_mat, bone, sides=6)

    def horror_eyes(self, name, center, eye_dist=0.09, radius=0.032, bone='head',
                    eye_mat='Eyes', socket_mat='Charcoal', gore_mat='Gore'):
        """Sunken weeping eye sockets with piercing reflective predatory corneas that flare in flashlights."""
        c = Vector(center)
        for sign in [-1, 1]:
            pos = c + Vector((sign * eye_dist * 0.5, 0, 0))
            # Deep sunken black orbit hollow
            self.ellipsoid(f'{name}_Socket_{sign}', pos + Vector((0, 0.02, 0)),
                           (radius * 2.3, radius * 1.8, radius * 2.2), socket_mat, bone)
            # Piercing retroreflective iris/pupil
            self.ellipsoid(f'{name}_Cornea_{sign}', pos + Vector((0, -0.012, 0)),
                           (radius * 1.3, radius * 1.1, radius * 1.3), eye_mat, bone)
            # Inner pinpoint black pupil
            self.ellipsoid(f'{name}_Pupil_{sign}', pos + Vector((0, -0.019, 0)),
                           (radius * 0.5, radius * 0.3, radius * 0.5), 'Charcoal', bone)
            # Weeping black bile/blood tear trail running down the cheekbone
            self.tube(f'{name}_Tear_{sign}', [
                (pos.x, pos.y - 0.01, pos.z - radius * 0.9, 0.007),
                (pos.x + sign * 0.008, pos.y - 0.02, pos.z - radius * 2.2, 0.006),
                (pos.x + sign * 0.014, pos.y - 0.028, pos.z - radius * 3.8, 0.004)
            ], gore_mat, bone, sides=5)

    def spinal_spikes(self, z_start, z_end, y_offset, count=9, spike_len=0.10,
                      material='Bone', bone='spine', gore_mat='Gore'):
        """Jagged, protruding spinal vertebrae and bone spikes bursting through split dorsal skin."""
        for i in range(count):
            t = i / max(1, count - 1)
            z = z_start + t * (z_end - z_start)
            y = y_offset + (math.sin(t * math.pi) * 0.03)
            base = Vector((0, y, z))
            # Protruding sharp spinous process jutting back and slightly upward
            tip = base + Vector((0, spike_len * (0.8 + math.sin(i * 1.7) * 0.3), 0.025))
            self.tube(f'SpinalSpike_{i}', [
                (base.x, base.y, base.z, 0.024),
                (base.x, (base.y + tip.y) * 0.5, (base.z + tip.z) * 0.5, 0.016),
                (tip.x, tip.y, tip.z, 0.004)
            ], material, bone, sides=6)
            # Split torn flesh ring at the base of the spike
            self.ellipsoid(f'SpikeSplitFlesh_{i}', base + Vector((0, 0.015, 0)),
                           (0.055, 0.035, 0.05), gore_mat, bone)

    def horror_hand(self, bone_key, hand_pos, arm_width, fingers=5, finger_len=0.28,
                    claw_len=0.08, material='Bone', claw_mat='Iron', sign=1):
        """Long, emaciated multi-jointed fingers ending in razor-sharp calcified talons."""
        hand = Vector(hand_pos)
        # Metacarpal palm base
        self.ellipsoid('EmaciatedPalm', hand + Vector((0, -0.02, -0.06)),
                       (arm_width * 1.25, arm_width * 0.75, 0.18), material, bone_key)
        
        # Fingers
        for d in range(fingers):
            u = (d / max(1, fingers - 1)) - 0.5
            start = hand + Vector((u * arm_width * 0.85, -0.025, -0.10))
            # 3 distinct articulated joint knuckles
            f_len = finger_len * (0.85 + (1.0 - abs(u) * 1.5) * 0.25)
            j1 = start + Vector((sign * 0.015, -0.045, -f_len * 0.38))
            j2 = j1 + Vector((sign * 0.025, -0.08, -f_len * 0.72))
            tip = j2 + Vector((sign * 0.02, -0.11, -f_len))
            # Sharp curved taloned claw extending from fingertip
            claw_tip = tip + Vector((sign * 0.01, -claw_len * 0.7, -claw_len * 0.75))
            
            # Finger bone tubes
            self.tube(f'DigitBone_{d}', [
                (*start, arm_width * 0.12),
                (*j1, arm_width * 0.095),
                (*j2, arm_width * 0.075),
                (*tip, arm_width * 0.05)
            ], material, bone_key, sides=6)
            # Knuckle nodes
            self.ellipsoid(f'KnuckleNode1_{d}', j1, (arm_width * 0.15, arm_width * 0.14, arm_width * 0.15), material, bone_key)
            self.ellipsoid(f'KnuckleNode2_{d}', j2, (arm_width * 0.12, arm_width * 0.11, arm_width * 0.12), material, bone_key)
            # Wicked claw
            self.tube(f'ClawTalon_{d}', [
                (*tip, arm_width * 0.06),
                (tip.x, tip.y - claw_len * 0.35, tip.z - claw_len * 0.35, arm_width * 0.04),
                (*claw_tip, 0.003)
            ], claw_mat, bone_key, sides=5)
