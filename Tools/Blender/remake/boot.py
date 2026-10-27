"""Load (or reload) the remake package inside a running Blender, live through the MCP or in batch mode.

    exec(open('/Users/zeyad/Documents/Abandoned/Tools/Blender/remake/boot.py').read())
    from remake.core import kit, materials, bake, export, rig, sculpt, fracture, polyhaven

Batch: Blender --background --factory-startup --python Tools/Blender/remake/<area>/<script>.py -- <args>
(each area script calls boot itself).
"""
import importlib
import os
import sys

_HERE = globals().get('__file__', '')
if not str(_HERE).endswith('boot.py'):
    # Exec'd without a file: the live Blender reads the mirror (sync_live.sh); batch reads the repository.
    _MIRROR = os.path.expanduser('~/Library/Caches/AbandonedRemake/src/remake/boot.py')
    _REPO_BOOT = '/Users/zeyad/Documents/Abandoned/Tools/Blender/remake/boot.py'
    try:
        open(_REPO_BOOT).close()
        _HERE = _REPO_BOOT
    except PermissionError:
        _HERE = _MIRROR
_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(_HERE)))
if _ROOT not in sys.path:
    sys.path.insert(0, _ROOT)
for _name in sorted([m for m in sys.modules if m == 'remake' or m.startswith('remake.')], key=len, reverse=True):
    importlib.reload(sys.modules[_name])
