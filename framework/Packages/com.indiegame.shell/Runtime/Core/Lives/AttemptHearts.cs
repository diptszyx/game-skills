namespace IndieGame.Shell
{
    // One heart per failed attempt: spent when the loss is shown or an unwon attempt is left, never twice.
    public sealed class AttemptHearts
    {
        readonly LivesService lives;
        bool owesHeart;
        public AttemptHearts(LivesService lives) { this.lives = lives ?? throw new System.ArgumentNullException(nameof(lives)); }

        public bool CanStart()
        {
            lives.Refresh();
            return lives.IsUnlimited || lives.Hearts > 0;
        }
        public void Begin() => owesHeart = true;
        public void Won() => owesHeart = false;
        public bool Lost() => Spend();
        public bool Left() => Spend();

        bool Spend()
        {
            if (!owesHeart) return false;
            owesHeart = false;
            return lives.SpendHeart().Success;
        }
    }
}
