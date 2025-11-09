using System.Reflection;
using MonoMod.Cil;
using ReLogic.Graphics;

namespace Terramon.Core.Systems;

/// <summary>
/// Fixes the rendering of the 'é' character.
/// The vanilla sprite fonts technically have the character in their gylph atlas,
/// but it uses comic sans or some other font instead of Andy Bold.
/// This makes the 'é' character map to 'e', and has a special check to draw the '`' above the e.
/// </summary>
public class FixedERenderingSystem : ModSystem
{
    private const char PokeE = 'é';

    public override void Load()
    {
        MonoModHooks.Modify(
            typeof(DynamicSpriteFont).GetMethod("GetCharacterData", BindingFlags.NonPublic | BindingFlags.Instance),
            ModifyGetCharacterData
        );
    }

    private static void ModifyGetCharacterData(ILContext il)
    {
        var c = new ILCursor(il);

        var label = c.DefineLabel();
        c.EmitLdarg1();
        c.EmitLdcI4('e');
        c.EmitCeq();
        c.EmitBrfalse(label);

        c.EmitLdarg0();
        c.EmitLdfld(typeof(DynamicSpriteFont).GetField("_spriteCharacters", BindingFlags.Instance | BindingFlags.NonPublic)!);
        c.EmitDelegate(static (Dictionary<char, DynamicSpriteFont.SpriteCharacterData> spriteCharacters) => spriteCharacters['e']);
        c.EmitRet();
        c.MarkLabel(label);
    }
}
