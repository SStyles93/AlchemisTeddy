/// <summary>
/// Defines the different categories an item can belong to.
/// Using an enum makes the code more readable and less prone to errors
/// than using strings or integers. It's perfect for a switch statement.
/// </summary>
public enum ItemType
{
    Ingredient,
    Potion,
    Key,
    Recipe,
    Generic
}
