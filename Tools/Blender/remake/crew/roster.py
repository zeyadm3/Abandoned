"""The eight playable workers (user, 2026-10-27: "different models, different characters").

Fictional people; never name anything after the user or a real person. All share the crew skeleton (eyes at
1.65 m), so faces differ in shape, colouring, age and hair, and bodies in build (outfits.BUILDS):
0 stocky, 1 average, 2 slim. Colours are sRGB.
"""

CHARACTERS = {
    'doyle': dict(
        name='Doyle', build=0, blurb='Thirty years of demolition. Has opinions about load-bearing walls.',
        tone=(.78, .58, .49), hair_colour=(.16, .14, .12), hair_grey=.5, iris=((.25, .32, .36), (.45, .52, .55)),
        face=dict(age=1.6, brow=1.1, jaw=1.15, chin=1.05, fold=1.2, hairline='receding', beard='short', fullness=1.1),
        style=None),
    'reyes': dict(
        name='Reyes', build=1, blurb='Ex-paramedic. Lifts with her legs and tells you about it.',
        tone=(.7, .5, .37), hair_colour=(.03, .02, .015), iris=((.18, .1, .05), (.32, .2, .1)),
        face=dict(scale=.95, width=.96, brow=.35, temples=.7, cheek=1.35, jaw=.45, chin=.5, chin_split=.3, nose_wing=.72,
                  nose_tip=.8, bridge=.8, nose_w=.88, lips=1.3, age=.55, philtrum=.8, hairline='long', brow_thick=.7),
        style='bun'),
    'mensah': dict(
        name='Mensah', build=0, blurb='Former structural engineer. Hears a floor before it goes.',
        tone=(.30, .19, .13), hair_colour=(.02, .015, .012), iris=((.1, .06, .03), (.2, .12, .06)),
        face=dict(age=1.15, brow=1.2, jaw=1.2, cheek=1.1, nose_w=1.18, nose_wing=1.2, bridge=.8, lips=1.35, mouth_w=1.08,
                  hairline='shaved', beard='full'),
        style=None, under=.45, beard_amount=.9),
    'halvorsen': dict(
        name='Halvorsen', build=2, blurb='Grew up on a fishing boat. Nothing in a basement scares her.',
        tone=(.88, .71, .63), hair_colour=(.55, .42, .25), iris=((.2, .33, .45), (.42, .55, .62)),
        face=dict(scale=.95, width=.94, depth=1.02, brow=.4, cheek=1.4, jaw=.55, chin=.6, nose_wing=.7, nose_tip=.85,
                  bridge=1.05, nose_w=.85, lips=1.05, age=1.0, temples=.8, hairline='long', brow_thick=.65),
        style='ponytail', freckles=.5),
    'novak': dict(
        name='Novak', build=1, blurb='Took the job for one summer, four years ago.',
        tone=(.82, .64, .54), hair_colour=(.12, .07, .04), iris=((.25, .3, .14), (.45, .42, .2)),
        face=dict(age=.35, brow=.9, jaw=.95, chin=1.1, cheek=.9, nose_tip=1.05, bridge=1.15, fullness=.9, lips=.95,
                  hairline='crew', beard='stubble'),
        style='messy', under=.9, beard_amount=.42),
    'chen': dict(
        name='Chen', build=2, blurb='Surveyor. Measures twice, carries once.',
        tone=(.84, .67, .53), hair_colour=(.02, .018, .016), iris=((.1, .06, .035), (.2, .13, .07)),
        face=dict(scale=.94, width=1.0, depth=.94, brow=.3, temples=.6, cheek=1.2, jaw=.5, chin=.55, nose_wing=.75,
                  nose_tip=.75, bridge=.55, nose_w=.95, lips=1.1, sockets=.7, age=.45, eye_x=.032, hairline='long',
                  brow_thick=.75),
        style='bob'),
    'brooks': dict(
        name='Brooks', build=0, blurb='Drove the truck for the old owners. Still does.',
        tone=(.36, .23, .16), hair_colour=(.4, .38, .36), hair_grey=.2, iris=((.12, .08, .05), (.24, .16, .09)),
        face=dict(age=1.9, brow=1.1, jaw=1.1, fullness=1.35, cheek=.85, nose_w=1.15, nose_wing=1.15, lips=1.2, fold=1.4,
                  chin=.9, hairline='fringe', beard='moustache'),
        style=None),
    'nair': dict(
        name='Nair', build=1, blurb='Electrician. Will rewire the breaker room if you let her.',
        tone=(.58, .4, .28), hair_colour=(.03, .02, .015), iris=((.1, .06, .03), (.22, .14, .07)),
        face=dict(scale=.95, width=.95, brow=.4, temples=.7, cheek=1.25, jaw=.5, chin=.55, nose_wing=.8, nose_tip=.95,
                  bridge=.95, nose_w=.95, lips=1.25, age=.7, hairline='long', brow_thick=.85),
        style='ponytail'),
}
