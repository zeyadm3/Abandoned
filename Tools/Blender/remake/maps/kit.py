"""The map kit (2026 remake, phase 6): one modelled prop per family and variant, standing in for the code-built copies
in every map (Editor/Remake/KitSwap matches them by name and size, Editor/Remake/KitFamilies lists them).

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/maps/kit.py -- [families...] [--preview]
Writes Art/Remake/Maps/Kit/Kit_<family>_<a|b|c|d>.fbx and Textures/Kit_<family>_<v>_*.png.
"""
import os
import sys

import bpy

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import kit, preview  # noqa: E402
from remake.maps import kit_furniture, kit_services, kit_street  # noqa: E402
from remake.maps.parts import Parts  # noqa: E402

FAMILIES = {**kit_furniture.FAMILIES, **kit_services.FAMILIES, **kit_street.FAMILIES}
BUDGET = {'car': 14000, 'water_tank': 8000, 'bookcase': 6000, 'bus_shelter': 5000}


def build(family, variant, show=False):
    kit.reset()
    maker, _, tex = FAMILIES[family]
    name = f'Kit_{family}_{"abcd"[variant]}'
    p = Parts()
    maker(p, variant)
    bpy.context.view_layer.update()
    ob = p.finish(name, tex=tex, budget=BUDGET.get(family, 4000))
    if show:
        preview.save(name)
    print(f'[remake-kit] {name} tris={kit.tris(ob)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    show = '--preview' in args
    wanted = [a for a in args if not a.startswith('--')] or list(FAMILIES)
    for family in wanted:
        fam, _, var = family.partition(':')
        variants = [int(var)] if var else range(FAMILIES[fam][1])
        for variant in variants:
            try:
                build(fam, variant, show)
            except Exception as e:  # noqa: BLE001 - one bad model must not stop the kit
                import traceback
                traceback.print_exc()
                print(f'[remake-kit] FAILED {fam}:{variant}: {e}')


if __name__ == '__main__':
    main()
