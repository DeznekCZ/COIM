
class LocStrFormatted:
  """Holds an value for translated text, this type is replacement for any of Mafi.Localization.*:
  LocStr, LocStr1, LocStr2, ..."""

  def __init__(self, id: str, text: str):
    self.Id = id
    self.TranslatedString = text

  def __getattribute__(self, name: str):
    return LocStr(self.Id, self.TranslatedString);

class LocStr:
  def __init__(self, id: str, text: str):
    LocStrFormatted.__init__(self, id, text)
LocStr.Empty = LocStr("__empty__","")

class LocStr1:
  Empty = LocStrFormatted

  def __init__(self, id: str, format: str):
    self.Id = id
    self.Format = format

  def Format(self, argument):
    return LocStrFormatted(self.Id, str.format(self.Format, argument))

LocStr1.Empty = LocStr1("__empty__","")
