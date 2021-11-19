namespace Transit.Templates {
    public class CardData {
        public string name;
        public string spritePath;
        public string description;
    }

    public static class CardTemplates {
        public static CardData SpawnPeds = new CardData() {
            name = "Spawn pedestrians",
            description = "Spawn pedestrians",
        };
        public static CardData BuildBlock = new CardData() {
            name = "Build block",
            description = "Build block",
        };
        public static CardData BuildShop = new CardData() {
            name = "Build shop",
            description = "Build shop",
        };
        public static CardData BuildHotel = new CardData() {
            name = "Build hotel",
            description = "Build hotel",
        };
    }
}