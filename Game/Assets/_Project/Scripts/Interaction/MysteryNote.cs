using Abandoned.Core;
using Abandoned.UI;
using UnityEngine;

namespace Abandoned.Interaction
{
    public class MysteryNote : MonoBehaviour,IUsable
    {
        [SerializeField] private string title;
        [SerializeField,TextArea] private string text;
        private ScreenPanel screen;
        private bool open;
        public string UsePrompt(GameObject user)=>"Read "+title;
        public void Use(GameObject user)=>open=true;
        private void Update()
        {
            if(open && UnityEngine.InputSystem.Keyboard.current!=null&&UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)open=false;
            CursorOwner.Set(this,open);
            if(screen==null){if(!open||(screen=ScreenPanel.Create(wide:false))==null)return;}
            screen.Show(open);
            if(open)screen.Build(title,p=>{MenuKit.Text(p,title.ToUpperInvariant(),"heading");MenuKit.Text(p,text);MenuKit.Button(p,"Fold it away",()=>open=false);});
        }
        private void OnDisable(){open=false;CursorOwner.Set(this,false);screen?.Show(false);}
        private void OnDestroy()=>screen?.Remove();
#if UNITY_EDITOR
        public void EditorSetup(string heading,string body){title=heading;text=body;}
#endif
    }
}
