from Mafi import ColorRgba, Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
from CustomAssets import build_product_fluid, add_texture

# coal vapor product used as intermediate in coal liquefaction
build_product_fluid(
    productId = "Product_CoalVapor",
    name = "Coal vapor",
    icon = add_texture("Assets/Products/Icons/CoalVapor.png"),
    description = "Is created by heating of coal",
    color = ColorRgba.DarkDarkGray,
    transportColor = ColorRgba.DarkGray,
    transportAccentColor = ColorRgba.CornflowerBlue,
    isStorable = False
)
