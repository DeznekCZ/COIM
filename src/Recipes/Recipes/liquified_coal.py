from Mafi import ColorRgba, Duration, Quantity, Vector2i
from Mafi.Base import Assets, Ids
from CustomAssets import build_product_fluid, add_texture

# coal vapor product used as intermediate in coal liquefaction
build_product_fluid(
    productId = "Product_CoalVapor",
    name = "Coal vapor (hot)",
    icon = add_texture("Assets/Products/Icons/CoalVapor.png"),
    description = "Is created by heating of coal without oxygen",
    color = ColorRgba.DarkDarkGray,
    transportColor = ColorRgba.DarkGray,
    transportAccentColor = ColorRgba.Orange,
    isStorable = False
)

# coal condensate product used as intermediate in coal liquefaction
build_product_fluid(
    productId = "Product_CoalCondensate",
    name = "Coal vapor (cooled)",
    icon = add_texture("Assets/Products/Icons/CoalCondensate.png"),
    description = "Is created by condensation of coal vapor",
    color = ColorRgba.DarkDarkGray,
    transportColor = ColorRgba.DarkGray,
    transportAccentColor = ColorRgba.CornflowerBlue,
    isStorable = False
)

# coal gas product used as intermediate in coal liquefaction
build_product_fluid(
    productId = "Product_CoalGasImpure",
    name = "Coal gas (impure)",
    icon = add_texture("Assets/Products/Icons/CoalGasImpure.png"),
    description = "Is created by coking of coal",
    color = ColorRgba.DarkDarkGray,
    transportColor = ColorRgba.DarkGray,
    transportAccentColor = ColorRgba.DarkYellow,
    isStorable = False
)

# # wood vapor product used as intermediate in wood liquefaction
# build_product_fluid(
#     productId = "Product_WoodVapor",
#     name = "Wood vapor (hot)",
#     icon = add_texture("Assets/Products/Icons/WoodVapor.png"),
#     description = "Is created by heating of wood without oxygen",
#     color = ColorRgba.DarkDarkGray,
#     transportColor = ColorRgba.Brown,
#     transportAccentColor = ColorRgba.Orange,
#     isStorable = False
# )
# 
# # wood condensate
# #build_product_fluid(
# #    productId = "Product_WoodCondensate",
# #    name = "Wood vapor (cooled)",
# #    icon = add_texture("Assets/Products/Icons/WoodCondensate.png"),
# #    description = "Is created by condensation of wood vapor",
# #    color = ColorRgba.Brown,
# #    transportColor = ColorRgba.DarkGray,
# #    transportAccentColor = ColorRgba.CornflowerBlue,
# #    isStorable = False
# #)
# 
# # wood gas product used as intermediate in wood liquefaction
# build_product_fluid(
#     productId = "Product_WoodGasImpure",
#     name = "Wood gas (impure)",
#     icon = add_texture("Assets/Products/Icons/WoodGasImpure.png"),
#     description = "Is created by condensation of wood vapor",
#     color = ColorRgba.DarkDarkGray,
#     transportColor = ColorRgba.Brown,
#     transportAccentColor = ColorRgba.DarkYellow,
#     isStorable = False
# )
