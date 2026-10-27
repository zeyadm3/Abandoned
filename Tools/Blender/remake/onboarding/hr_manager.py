"""The HR manager of the hiring scene (2026 remake, phase 7): Dana Whitfield, Human Resources at Abandoned Salvaging
(a fictional person). Built with the crew's own pipeline (crew/characters.py, crew/outfit_build.py) on the crew
skeleton, but written to Art/Remake/Onboarding/Characters so she never joins the playable roster.

Batch:
  Blender --background --factory-startup --python Tools/Blender/remake/onboarding/hr_manager.py -- [--preview]
"""
import os
import sys

_HERE = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, os.path.dirname(os.path.dirname(_HERE)))
exec(open(os.path.join(os.path.dirname(_HERE), 'boot.py')).read())

from remake.core import export, materials  # noqa: E402
from remake.crew import characters, outfit_build, outfits  # noqa: E402

OUT = export.art_dir('Onboarding', 'Characters')
TEX = export.art_dir('Onboarding', 'Characters', 'Textures')

WHITFIELD = dict(
    name='Dana Whitfield', build=1, blurb='Human Resources, Abandoned Salvaging.',
    tone=(.76, .57, .47), hair_colour=(.28, .19, .12), hair_grey=.35, iris=((.2, .24, .14), (.38, .4, .24)),
    face=dict(scale=.95, width=.95, brow=.4, temples=.75, cheek=1.15, jaw=.55, chin=.6, nose_wing=.75, nose_tip=.9,
              bridge=1.0, nose_w=.9, lips=1.0, age=1.35, fold=.9, hairline='long', brow_thick=.6),
    style='bun')

# A dark jacket over grey trousers, office shoes, and her own hands (the crew wear work gloves).
OFFICE = dict(name='Office', blurb='', jacket=('twill', (.1, .11, .16)), trousers=(.2, .2, .21), vest=None, boots='work',
              gloves='skin', tint=(), price=0)


def _with_hands(original):
    def patched(spec):
        M = original(spec)
        if spec.get('gloves') == 'skin':
            M['Glove'] = M['GlovePalm'] = materials.skin('HrHands', WHITFIELD['tone'])
        return M
    return patched


def main():
    show = '--preview' in sys.argv
    characters.OUT = outfit_build.OUT = OUT
    characters.TEX = outfit_build.TEX = TEX
    outfits.materials = _with_hands(outfits.materials)
    characters.build('whitfield', WHITFIELD, show)
    outfit_build.build('office', OFFICE, show)


if __name__ == '__main__':
    main()
