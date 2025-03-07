namespace EviCache.Tests.Helpers;

public class CollisionKey(int id)
{
    private readonly int _id = id;

    public override int GetHashCode()
    {
        return 25;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        CollisionKey other = (CollisionKey)obj;
        return _id == other._id;
    }

    public override string ToString()
    {
        return $"Key {_id}";
    }
}