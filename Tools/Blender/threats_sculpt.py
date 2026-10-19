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
