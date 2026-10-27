"""The HQ's two working stations, remodelled (2026 remake): the crew-issue gear rack and the supply shop terminal.
Each is drawn in its station's own space so it drops in at the origin: the rack (HqOperationsStations.Workshop's
shelf) 2.4 m wide, 2.4 m tall, 0.7 m deep, standing on y 0, its open front at -Z (Blender -Y); the terminal centred
on its 0.8 x 0.5 x 0.3 block, the screen (TerminalFace's amber CRT, kept) at local (-0.04, 0.025, -0.18) facing -Z.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/hq/hq_stations.py -- [gear_rack shop_terminal] [--preview]
"""
import math
import os
import sys

import bpy

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import bake, export, kit, preview  # noqa: E402
from remake.maps.parts import Parts  # noqa: E402


def gear_rack(p):
    W, H, D = 2.4, 2.4, .7
    frame = p.paint('RackGrey', (.28, .3, .3), wear=.5)
    accent = p.paint('RackOrange', (.86, .42, .1), wear=.55)
    for x in (-W / 2 + .04, W / 2 - .04):
        for y in (-D / 2 + .04, D / 2 - .04):
            p.slab(frame, x - .04, x + .04, y - .04, y + .04, 0, H)
    # Slatted back wall with hooks, a header plate with the company stripe, three decks below.
    for i in range(14):
        z = .95 + i * .1
        p.slab('Galvanised', -W / 2 + .08, W / 2 - .08, D / 2 - .06, D / 2 - .03, z, z + .07, bevel=.004)
    p.slab(frame, -W / 2, W / 2, -D / 2 - .02, -D / 2 + .02, H - .26, H - .02, bevel=.006)
    p.slab(accent, -W / 2 + .02, W / 2 - .02, -D / 2 - .03, -D / 2 - .02, H - .2, H - .14)
    for k in range(3):
        z = .12 + k * .38
        p.slab('Galvanised', -W / 2 + .04, W / 2 - .04, -D / 2 + .04, D / 2 - .04, z, z + .04, bevel=.004)
        p.slab(accent if k == 0 else frame, -W / 2 + .04, W / 2 - .04, -D / 2 + .01, -D / 2 + .05, z - .03, z + .05)
    # Bins on the decks.
    colours = ('PlasticYellow', 'PlasticGrey', 'PlasticRed', 'PlasticGrey')
    for k in range(3):
        z = .16 + k * .38
        for i in range(4):
            x = -W / 2 + .35 + i * .57
            bin_ = colours[(i + k) % 4]
            p.slab(bin_, x - .24, x + .24, -D / 2 + .08, D / 2 - .12, z, z + .24, bevel=.012)
            p.slab(bin_, x - .1, x + .1, -D / 2 + .05, -D / 2 + .09, z + .14, z + .2, bevel=.006)
    # Hooks and the gear on them: flashlights, radios, a crowbar, medkits, battery packs on a charging strip.
    for i in range(10):
        x = -W / 2 + .25 + i * .21
        p.rod('Steel', (x, D / 2 - .05, 2.0), (x, D / 2 - .2, 2.02), .008)
    for i, x in enumerate((-.95, -.74, -.53)):
        p.cyl('PaintBlack', .035, .26, (x, D / 2 - .2, 1.85), sides=12)
        p.cyl('Chrome', .045, .05, (x, D / 2 - .2, 1.71), sides=12)
    for x in (-.25, -.04):
        p.box('Plastic', (.08, .05, .22), (x, D / 2 - .18, 1.82), bevel=.01)
        p.rod('PaintBlack', (x + .025, D / 2 - .18, 1.93), (x + .025, D / 2 - .18, 2.05), .005)
    p.rod('PaintRed', (.2, D / 2 - .18, 1.35), (.38, D / 2 - .18, 2.0), .014)
    for x in (.62, .85):
        p.box('PlasticRed', (.18, .08, .14), (x, D / 2 - .16, 1.85), bevel=.015)
        p.box('PaintWhite', (.05, .085, .015), (x, D / 2 - .2, 1.85))
    p.slab('PlasticGrey', -W / 2 + .1, W / 2 - .1, D / 2 - .14, D / 2 - .06, 1.12, 1.2, bevel=.008)
    for i in range(8):
        x = -W / 2 + .3 + i * .26
        p.box('PaintBlack', (.08, .06, .12), (x, D / 2 - .14, 1.27), bevel=.008)
        p.box('EmitGreen', (.012, .012, .012), (x + .025, D / 2 - .175, 1.3))
    p.rod('PaintBlack', (W / 2 - .1, D / 2 - .1, 1.16), (W / 2 - .1, D / 2 - .1, 0), .012)


def shop_terminal(p):
    steel = p.paint('TerminalSteel', (.32, .34, .33), wear=.5)
    trim = p.paint('TerminalOrange', (.86, .42, .1), wear=.6)
    # Housing round the block, its front recessed round the CRT, a rain hood over it.
    p.slab(steel, -.42, .42, -.13, .17, -.3, .32, bevel=.012)
    for x0, x1, z0, z1 in ((-.42, -.37, -.3, .32), (.29, .42, -.3, .32), (-.37, .29, .18, .32), (-.37, .29, -.3, -.13)):
        p.slab(steel, x0, x1, -.17, -.13, z0, z1, bevel=.006)
    p.slab('Rubber', -.37, .29, -.165, -.15, -.13, .18)
    p.box(steel, (.92, .32, .03), (0, -.08, .37), bevel=.008, rot=(-12, 0, 0))
    p.slab(trim, -.42, .42, -.175, -.17, .3, .32)
    # Keypad and card slot below; a receipt printer at the side; switches.
    p.slab(steel, -.3, .22, -.3, -.13, -.42, -.32, bevel=.008)
    for r in range(3):
        for c in range(4):
            p.slab('PaintBlack', -.25 + c * .07, -.2 + c * .07, -.28, -.25, -.4 + r * .028, -.38 + r * .028, bevel=.002)
    p.slab('PaintBlack', .08, .18, -.275, -.255, -.36, -.35)
    p.slab(steel, .3, .42, -.16, .14, -.3, -.16, bevel=.008)
    p.slab('Paper', .33, .39, -.17, -.165, -.42, -.24)
    for z in (.1, .03, -.04):
        p.slab('Aluminium', .32, .36, -.18, -.17, z - .012, z + .012, bevel=.002)
    p.box('EmitWarm', (.02, .01, .02), (.37, -.18, .14))
    # Conduit down to the floor and up to the cable tray.
    p.rod('Galvanised', (.35, .12, -.3), (.35, .12, -1.4), .025)
    p.rod('Galvanised', (-.35, .12, .32), (-.35, .12, 1.6), .025)
    p.slab(steel, .3, .4, .08, .16, -1.4, -1.36)


BUILDS = {'gear_rack': (gear_rack, 1024), 'shop_terminal': (shop_terminal, 512)}


def build(name, show=False):
    kit.reset()
    maker, tex = BUILDS[name]
    p = Parts()
    maker(p)
    bpy.context.view_layer.update()
    ob = kit.join(p.obs, f'Hq_{name}')
    kit.auto_smooth(ob, 35)
    kit.decimate_to(ob, 8000, delimit=('UV', 'MATERIAL', 'NORMAL'))
    bake.bake_asset(ob, f'Hq_{name}', export.art_dir('Maps', 'HQ', 'Textures'), size=tex, samples=10)
    export.static_asset(f'Hq_{name}', [ob], 'Maps/HQ', lods=(1.0, .45, .15))
    if show:
        preview.save(f'Hq_{name}')
    print(f'[remake-hq] {name} tris={kit.tris(ob)}')


def main():
    args = sys.argv[sys.argv.index('--') + 1:] if '--' in sys.argv else []
    for name in [a for a in args if not a.startswith('--')] or list(BUILDS):
        build(name, '--preview' in args)


if __name__ == '__main__':
    main()
