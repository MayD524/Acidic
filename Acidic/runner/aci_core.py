import UPL

## key = {"type" : type, "value" : value}
class aci_mem:
    def __init__(self):
        self.env = { }
        self.AddingOpcodes = [0x01, 0x02, 0x03, 0x04, 0x05, 0x06]
        self.BuiltInFuncs = [ 0x21, 0x25 ]

    def declairVar(self, varType, key, value):
        if varType == "aci_string":
            value = str(value)
            if "_" in value:
                value = value.replace('_', " ")

            
        elif varType == "aci_int" or varType == "aci_long":
            ops = ["+", "-", "*", "/"]
            op = [x for x in ops if x in value]

            if len(op) > 0:
                op = op[0]
                if op in value:
                    tmp = value.split(op)
                    for i in range(len(tmp)):
                        if tmp[i] in self.env.keys():
                            tmp[i] = str(self.env[tmp[i]]['value'])
                    value = f"{op}".join(tmp)

            value = eval(value)

        elif varType == "aci_float":
            value = eval(value)

        elif varType == "aci_bool":
            if value == "1" or value == "True":
                value == True
            else:
                value = False

        self.env[key] = { "type" : varType, "value" : value }


    def parse(self, act, actText, data:list) -> UPL.void:
        if act in self.AddingOpcodes:
            var = data[0]
            data = "".join(UPL.Core.removeVal(data, var))
            self.declairVar(varType=actText,key=var, value=data)
        
        elif act in self.BuiltInFuncs:
            if act == 0x21:
                mode = int(data[0])
                if mode == 0:
                    data = data[1]
                    if data in self.env.keys():
                        data = self.env[data]['value']

                print(data)

            elif act == 0x25:
                if data[0] in self.env.keys():
                    dType = self.env[data[0]]['type']
                    self.declairVar(dType, data[0], data[1])