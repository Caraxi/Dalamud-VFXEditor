using System;
using ImGuiNET;
using VFXEditor.AVFXLib;
using VFXEditor.Data;

namespace VFXEditor.Avfx.Vfx {
    public class UICombo<T> : UIBase {
        public readonly string Name;
        public int ValueIdx;
        public readonly AVFXEnum<T> Literal;
        public readonly Action OnChange;

        public UICombo( string name, AVFXEnum<T> literal, Action onChange = null ) {
            Name = name;
            Literal = literal;
            OnChange = onChange;
            ValueIdx = Array.IndexOf( Literal.Options, Literal.GetValue().ToString() );
        }

        public override void Draw( string id ) {
            if( CopyManager.IsCopying ) {
                CopyManager.Copied[Name] = Literal;
            }

            if( CopyManager.IsPasting && CopyManager.Copied.TryGetValue( Name, out var _literal ) && _literal is AVFXEnum<T> literal ) {
                Literal.SetValue( literal.GetValue() );
                ValueIdx = Array.IndexOf( Literal.Options, Literal.GetValue().ToString() );
                OnChange?.Invoke();
            }

            PushAssignedColor( Literal.IsAssigned() );
            var text = ValueIdx == -1 ? "[NONE]" : Literal.Options[ValueIdx];
            if( ImGui.BeginCombo( Name + id, text ) ) {
                for( var i = 0; i < Literal.Options.Length; i++ ) {
                    var isSelected = ( ValueIdx == i );
                    if( ImGui.Selectable( Literal.Options[i], isSelected ) ) {
                        ValueIdx = i;
                        Literal.SetValue( Literal.Options[ValueIdx] );
                        OnChange?.Invoke();
                    }

                    if( isSelected ) ImGui.SetItemDefaultFocus();
                }
                ImGui.EndCombo();
            }
            PopAssignedColor();
        }
    }
}
