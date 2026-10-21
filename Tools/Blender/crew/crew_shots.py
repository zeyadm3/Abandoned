"""Review shots beyond the turnaround: face close-up, the crew line-up, readability in the dark."""
import math
import crew_geo as G
import crew_materials as CM


def shots(C):
    def portrait():
        C.reset()
        C.studio(.7)
        a = C.crew('orange', 1, (-.36, 0, 0), 22)
        C.pose(a, head=-3)
        b = C.crew('blue', 2, (.36, 0, 0), -22, hat='hardhat')
        C.pose(b, head=-2)
        C.light('FaceKey', (-1.4, -2.2, 2.2), (0, 0, 1.6), 80, (1.0, .9, .8), 1.0)
        C.camera((0, -3.0, 1.66), (0, 0, 1.56), 85)
        C.render('crew_portraits', (2400, 1350))

    def face():
        C.reset()
        C.studio(.55)
        a = C.crew('orange', 1, (0, 0, 0), 28)
        C.pose(a)
        C.light('FaceKey', (-.9, -1.2, 2.0), (0, 0, 1.64), 45, (1.0, .9, .8), .6)
        C.light('FaceRim', (.9, .6, 1.9), (0, 0, 1.65), 30, (.75, .85, 1.0), .3)
        C.camera((0, -1.05, 1.67), (0, 0, 1.635), 85)
        C.render('crew_face', (1600, 1600))

    def lineup():
        C.reset()
        C.studio()
        a = C.crew('orange', 1, (-1.6, .1, 0), 12, hat='hardhat')
        C.pose(a, arms=((4, 10), (-6, 10)), head=-3)
        b = C.crew('blue', 2, (-.55, .45, 0), 4, hat='beanie')
        C.pose(b, arms=(0, (-160, 22)), elbows=(-10, -35), legs=(-8, 6), knees=(4, 0), head=-6)
        c = C.crew('yellow', 3, (.55, .1, 0), -8)
        C.pose(c, arms=((-40, 3), (-40, 3)), elbows=(-40, -40), torso=3, legs=(14, -12), knees=(0, 8))
        crate(c['torso'])
        d = C.crew('navy', 4, (1.6, .45, 0), -18, hat='cap')
        C.pose(d, crouch=1.0, arms=((-30, 12), (-24, 12)), elbows=(-30, -25), head=-10)
        C.light('Wash', (0, 1.0, 4.2), (0, 4.0, 1.5), 260, (1.0, .92, .82), 4.0)
        C.camera((1.0, -5.7, 1.2), (0, .25, .82), 40)
        C.render('crew_lineup')

    def dark():
        C.reset()
        C.world((0.0015, 0.0016, 0.002), 1.0)
        corridor()
        a = C.crew('orange', 1, (-.45, 3.2, 0), 180, hat='hardhat')
        C.pose(a, legs=(18, -16), knees=(4, 18), arms=(-12, 10), elbows=(-15, -12), twist=-6)
        b = C.crew('teal', 3, (.55, 4.6, 0), 200)
        C.pose(b, legs=(-14, 12), knees=(16, 2), arms=(10, -12), elbows=(-12, -18), head=6)
        c = C.crew('yellow', 2, (.2, 8.5, 0), 20, hat='beanie')
        C.pose(c, arms=((-70, 4), 6), elbows=(-25, -10), legs=(-6, 4))
        C.light('Flashlight', (.25, -.4, 1.55), (0, 4.5, .9), 260, (1.0, .93, .8), .02, kind='SPOT', spot=34)
        C.light('Beam', (.25, 8.1, 1.25), (-.3, 3.5, .6), 70, (1.0, .9, .75), .02, kind='SPOT', spot=26)
        C.light('Spill', (0, 12.0, 2.6), (0, 6, 0), 25, (.55, .65, .85), 2.0)
        C.camera((-.15, -.6, 1.6), (.05, 6.0, 1.0), 30)
        fog()
        C.render('crew_dark')

    return {'portrait': portrait, 'face': face, 'lineup': lineup, 'dark': dark}


def crate(torso):
    """A salvage box held in both hands (shoulders -40, elbows -40)."""
    box = G.rbox('Crate', (0, -.5, 1.1), (.34, .3, .26), CM.plain('Cardboard', (0.45, 0.32, 0.18), .8), .01, subsurf=0)
    tape = G.rbox('CrateTape', (0, -.5, 1.231), (.07, .305, .004), CM.plain('PackTape', (0.75, 0.62, 0.38), .3), 0)
    for ob in (box, tape):
        G.attach(ob, torso)


def corridor():
    """A plain dark service corridor, 3 m wide, so the shot is about the crew, not the set."""
    wall = CM.Graph('Concrete')
    n = wall.noise(4.0, 8.0, .6).outputs['Fac']
    wall.set('Base Color', wall.mix((0.16, 0.155, 0.145, 1), (0.07, 0.068, 0.062, 1), wall.ramp(n, .35, .7)))
    wall.set('Roughness', .9)
    wall.set('Normal', wall.bump(wall.noise(40.0, 6.0).outputs['Fac'], .3, .01))
    for name, at, size in (('Floor', (0, 6, -.05), (3.2, 18, .1)), ('Ceiling', (0, 6, 3.05), (3.2, 18, .1)),
                           ('WallL', (-1.6, 6, 1.5), (.1, 18, 3.2)), ('WallR', (1.6, 6, 1.5), (.1, 18, 3.2)),
                           ('End', (0, 15, 1.5), (3.2, .1, 3.2))):
        G.rbox(name, at, size, wall.mat, 0)
    for y in (2.0, 6.0, 10.0):
        G.rbox('Pillar', (-1.45, y, 1.5), (.25, .35, 3.0), wall.mat, .02)
        G.rbox('Pillar', (1.45, y + 2, 1.5), (.25, .35, 3.0), wall.mat, .02)


def fog():
    import bpy
    w = bpy.context.scene.world
    vol = w.node_tree.nodes.new('ShaderNodeVolumePrincipled')
    vol.inputs['Density'].default_value = .035
    vol.inputs['Anisotropy'].default_value = .5
    w.node_tree.links.new(vol.outputs[0], w.node_tree.nodes['World Output'].inputs['Volume'])
    bpy.context.scene.cycles.volume_step_rate = 4.0
