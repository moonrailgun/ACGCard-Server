namespace CardServerControl.Model.DTO.GameData
{
    //游戏数据通用基类
    //TCP数据传输
    class GameData
    {
        public int roomID;//房间名
        public int returnCode;//返回值
        public int operateCode;//游戏操作名
        public string operateData;//操作数据

        public GameData()
        {
            operateData = "";
        }
    }
}
