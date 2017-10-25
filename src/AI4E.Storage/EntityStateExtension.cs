namespace AI4E.Storage
{
    public static class EntityStateExtension
    {
        public static bool IsLegal(this EntityState entityState)
        {
            return entityState >= 0 && entityState <= (EntityState)3;
        }
    }
}
