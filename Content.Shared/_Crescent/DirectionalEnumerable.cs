namespace Content.Shared._Crescent;


public enum DirectionalType
{
    None = 0, // No directional type , yaml error.
    Same = 1, // only connects/forms edges with neighboring tiles that are of the same tileId
    Exist = 2, // will form edges with any neighboring tile as long as its not of the Id Space
    ExistReversed = 3, // any "neighbors" would be considered as disconnected so that it may form edges with them that show as connections.
}
