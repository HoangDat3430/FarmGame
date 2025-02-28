public class ItemList : CSVReader
{
    public class RowData
    {
        public int itemID;
        public string itemName;
        public int buyPrice;
        public int sellPrice;
        public string prefabPath;
        public string iconPath;
        public int stackSize;
        public int cropID;
    }
    public RowData[] Table = null;
    public override void InitRowData(int line, string[] data)
    {
        Table[line] = new RowData();
        Table[line].itemID = int.Parse(data[0]);
        Table[line].itemName = data[1];
        Table[line].buyPrice = int.Parse(data[2]);
        Table[line].sellPrice = int.Parse(data[3]);
        Table[line].prefabPath = data[4];
        Table[line].iconPath = data[5];
        Table[line].stackSize = int.Parse(data[6]);
        Table[line].cropID = int.Parse(data[7]);
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
}
