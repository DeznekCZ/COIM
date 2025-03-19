class ReadOnlySubStream:

    def __init__(self):
        self.IsDone = False
        self.CanWrite = False
        self.CanRead = False
        self.Length = None
        self.CanSeek = False
        self.Position = None
        self.CanTimeout = False
        self.ReadTimeout = int(0)
        self.WriteTimeout = int(0)
