
class ReadOnlySubStream:
    def __init__(self):
        self.IsDone = False
        self.CanWrite = False
        self.CanRead = False
        self.Length = 0
        self.CanSeek = False
        self.Position = 0
        self.CanTimeout = False
        self.ReadTimeout = 0
        self.WriteTimeout = 0
