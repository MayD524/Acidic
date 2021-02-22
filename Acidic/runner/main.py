import aci_core
import UPL

class Acidic:
    def __init__(self, lines, filename):
        self.lines = lines
        self.filename = filename
        self.MemClass = aci_core.aci_mem()
        self.currentLine = 0
        self.saveState = 0 ## for goto command - goes here on return

        self.keyWords = {
            0x01 : "aci_int",
            0x02 : "aci_long",
            0x03 : "aci_float",
            0x04 : "aci_bool",
            0x05 : "aci_string",
            0x06 : "aci_char",
            0x20 : "func",
            0x21 : "out",
            0x25 : "set",
            0x26 : "goto",
            0x34 : "return",
            24576: "EOF"
        }

    def parser(self, line:list):
        cmd = int(line[0])
        aci_call = UPL.Core.switch(self.keyWords, cmd)
        if aci_call == "EOF":
            exit(UPL.EXIT_SUCCESS)
        line.remove(line[0])
        self.MemClass.parse(cmd, aci_call, line)


    def main(self):
        data = self.lines.split(";")
        while self.currentLine < len(data):
            line = data[self.currentLine].split(" ")
            self.parser(line)
            self.currentLine += 1

        


if __name__ == "__main__":
    filename = "test.acc"
    if UPL.Core.file_exists(filename):
        fileData = UPL.Core.file_manager.read_file(filename)[0]
        ACI = Acidic(fileData, filename)
        ACI.main()
    else:
        raise Exception(f"Cannot find the file '{filename}'")
