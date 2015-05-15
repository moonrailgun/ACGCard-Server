namespace CardServerControl.Model.DTO.GameData
{
    class AttackData : GameDetailData
    {
        public string fromCardUUID;
        public string toCardUUID;
        public int damage;
    }
}
