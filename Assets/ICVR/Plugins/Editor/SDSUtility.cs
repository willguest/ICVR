
using UnityEditor;

public static class SDSUtility
{
    public static string[] GetSymbols(BuildTargetGroup group)
    {
        return PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';');
    }

    public static void SetSymbols(BuildTargetGroup group, string[] symbols)
    {
        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols));
    }

    public static void AddSymbol(BuildTargetGroup group, string symbol)
    {
        var symbols = GetSymbols(group);
        if (System.Array.IndexOf(symbols, symbol) == -1)
        {
            System.Array.Resize(ref symbols, symbols.Length + 1);
            symbols[symbols.Length - 1] = symbol;
            SetSymbols(group, symbols);
        }
    }

    public static void RemoveSymbol(BuildTargetGroup group, string symbol)
    {
        var symbols = GetSymbols(group);
        var index = System.Array.IndexOf(symbols, symbol);
        if (index != -1)
        {
            var newSymbols = new string[symbols.Length - 1];
            System.Array.Copy(symbols, 0, newSymbols, 0, index);
            if (index < newSymbols.Length)
            {
                System.Array.Copy(symbols, index + 1, newSymbols, index, newSymbols.Length - index);
            }
            SetSymbols(group, newSymbols);
        }
    }

    public static bool ContainsSymbol(BuildTargetGroup group, string symbol)
    {
        var symbols = GetSymbols(group);
        return System.Array.IndexOf(symbols, symbol) != -1;
    }

}
