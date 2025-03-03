using System.Collections.Generic;

public class ItemList : CSVReader
{
    private Dictionary<int, RowData> itemDic = new Dictionary<int, RowData>();
    public class RowData
    {
        public int ItemID;
        public string ItemName;
        public int BuyPrice;
        public int SellPrice;
        public string PrefabPath;
        public string IconPath;
        public int MaxStackSize;
        public int CropID;
        public bool Consumable;
        public string Animator;
        public bool WholeSale;
        public int Type;
    }
    public RowData[] Table = null;
    public override void InitRowData(int line, string[] data)
    {
        Table[line] = new RowData();
        Table[line].ItemID = int.Parse(data[0]);
        Table[line].ItemName = data[1];
        Table[line].BuyPrice = int.Parse(data[2]);
        Table[line].SellPrice = int.Parse(data[3]);
        Table[line].PrefabPath = data[4];
        Table[line].IconPath = data[5];
        Table[line].MaxStackSize = int.Parse(data[6]);
        Table[line].CropID = int.Parse(data[7]);
        Table[line].Consumable = data[8] == "0" ? false : true;
        Table[line].Animator = data[9];
        Table[line].WholeSale = data[10] == "0" ? false : true;
        Table[line].Type = int.Parse(data[11]);
        itemDic[Table[line].ItemID] = Table[line];
    }
    public override void OnClear(int lineCount)
    {
        if (lineCount > 0)
        {
            Table = new RowData[lineCount];
        }
        else
        {
            Table = null;
        }
    }
    public RowData GetByItemId(int itemId)
    {
        return itemDic[itemId];
    }
}
