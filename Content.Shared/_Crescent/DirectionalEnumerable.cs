namespace Content.Shared._Crescent;


public enum DirectionalType
{
    None = 0, // No directional type , yaml error.
    Same = 1, // only connects/forms edges with neighboring tiles that are of the same tileId
    Exist = 2 // will form connections with any neighboring tile as long as its not of the Id Space
}
