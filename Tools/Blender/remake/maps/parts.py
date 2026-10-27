"""A small modelling vocabulary for the map kit (kit.py): boxes by their extents, posts, rails, turned parts, all with
real bevels and a material each. Blender space: X is the prop's length, +Y its front (Unity +Z), Z up, standing on
z 0 and centred on x and y. finish() joins, bakes one texture set and exports with LODs."""
import math

from mathutils import Vector

from remake.core import bake, export, kit, materials


class Parts:
    def __init__(self):
        self.obs = []
        self.M = materials.library()
        self._made = {}

    # ---------------------------------------------------------------------------------------------- materials
    def mat(self, key):
        return self.M[key]

    def paint(self, key, colour, wear=.45, rough=.45):
        if key not in self._made:
            self._made[key] = materials.paint('K_' + key, colour, wear=wear, rough=rough)
        return self._made[key]

    def fabric(self, key, colour):
        if key not in self._made:
            self._made[key] = materials.fabric('K_' + key, colour)
        return self._made[key]

    def surface(self, key, colour, rough=.6, metal=0.0, **kw):
        if key not in self._made:
            self._made[key] = materials.surface('K_' + key, colour, rough, metal, **kw)
        return self._made[key]

    def _m(self, m):
        return self.M[m] if isinstance(m, str) else m

    def _add(self, ob, m):
        materials.assign(ob, self._m(m))
        self.obs.append(ob)
        return ob

    # ---------------------------------------------------------------------------------------------- shapes
    def slab(self, m, x0, x1, y0, y1, z0, z1, bevel=.004):
        """A box by its extents."""
        size = (abs(x1 - x0), abs(y1 - y0), abs(z1 - z0))
        b = min(bevel, min(size) * .3)
        return self._add(kit.box('p', size, ((x0 + x1) / 2, (y0 + y1) / 2, (z0 + z1) / 2), bevel=b), m)

    def box(self, m, size, at, bevel=.004, rot=(0, 0, 0)):
        b = min(bevel, min(size) * .3)
        return self._add(kit.box('p', size, at, bevel=b, rotation=rot), m)

    def cyl(self, m, r, h, at, axis='Z', sides=16, bevel=.003):
        return self._add(kit.cylinder('p', r, h, at, sides=sides, axis=axis, bevel=bevel), m)

    def rod(self, m, a, b, r=.012, sides=8):
        return self._add(kit.rod('p', a, b, radius=r, sides=sides), m)

    def tube(self, m, points, r=.012, sides=8):
        return self._add(kit.tube_along('p', points, r, sides=sides), m)

    def lathe(self, m, profile, at=(0, 0, 0), sides=24):
        return self._add(kit.lathe('p', profile, sides=sides, location=at), m)

    def sphere(self, m, r, at, scale=(1, 1, 1), segments=16, rings=8):
        return self._add(kit.sphere('p', r, at, segments=segments, rings=rings, scale=scale), m)

    def cone(self, m, r1, r2, h, at, sides=16, axis='Z'):
        return self._add(kit.cone('p', r1, r2, h, at, sides=sides, axis=axis), m)

    def outline(self, m, points, depth, at=(0, 0, 0), axis='Y', bevel=0.0):
        """A flat outline (x, z pairs when axis is Y) extruded `depth` along axis: side profiles, signs, frames."""
        return self._add(kit.extrude_outline('p', points, depth, location=at, axis=axis, bevel=bevel), m)

    def legs(self, m, w, d, h, inset=.03, r=.018, square=False, z0=0.0):
        """Four legs under a top of w x d whose underside is at height h."""
        for sx in (-1, 1):
            for sy in (-1, 1):
                x, y = sx * (w / 2 - inset), sy * (d / 2 - inset)
                if square:
                    self.slab(m, x - r, x + r, y - r, y + r, z0, h)
                else:
                    self.cyl(m, r, h - z0, (x, y, (h + z0) / 2), sides=10, bevel=.002)

    def frame(self, m, x0, x1, z0, z1, y, t, depth):
        """A rectangular frame in the XZ plane (window and door casings, picture frames), bar width t."""
        self.slab(m, x0, x1, y - depth / 2, y + depth / 2, z0, z0 + t)
        self.slab(m, x0, x1, y - depth / 2, y + depth / 2, z1 - t, z1)
        self.slab(m, x0, x0 + t, y - depth / 2, y + depth / 2, z0 + t, z1 - t)
        self.slab(m, x1 - t, x1, y - depth / 2, y + depth / 2, z0 + t, z1 - t)

    # ---------------------------------------------------------------------------------------------- finishing
    def finish(self, name, tex=512, budget=None, lods=(1.0, .45, .15), samples=10):
        ob = kit.join(self.obs, name)
        kit.auto_smooth(ob, 35)
        if budget:
            kit.decimate_to(ob, budget, delimit=('UV', 'MATERIAL', 'NORMAL'))
        bake.bake_asset(ob, name, export.art_dir('Maps', 'Kit', 'Textures'), size=tex, samples=samples)
        export.static_asset(name, [ob], 'Maps/Kit', lods=lods)
        return ob


def arc(cx, cz, r, a0, a1, n=8):
    """Points along an arc in the XZ plane (degrees)."""
    return [(cx + r * math.cos(math.radians(a0 + (a1 - a0) * i / n)), cz + r * math.sin(math.radians(a0 + (a1 - a0) * i / n)))
            for i in range(n + 1)]


def v(*a):
    return Vector(a)
