import sys
import os
import re

lut = {
    "R0": 0,
    "R1": 1,
    "R2": 2,
    "R3": 3,
    "R4": 4,
    "R5": 5,
    "R6": 6,
    "R7": 7,
    "R8": 8,
    "R9": 9,
    "R10": 10,
    "R11": 11,
    "R12": 12,
    "R13": 13,
    "R14": 14,
    "R15": 15,
    "SCREEN": 16384,
    "KBD": 24576,
    "SP": 0,
    "LCL": 1,
    "ARG": 2,
    "THIS": 3,
    "THAT": 4,
}

currentAddr = 16
currentCmd = 0

def main():
    global lut
    global currentAddr
    global currentCmd
    global outfile
    global infile

    if len(sys.argv) < 2:
        print("No file specified")
        exit(-1)
    elif not sys.argv[1].endswith(".asm"):
        print("File is not assembly")
        exit(-1)

    infile = open(sys.argv[1], "r")

    if os.path.isfile(sys.argv[1].replace(".asm", ".hack")):
        outfile = open(sys.argv[1].replace(".asm", ".hack"), "w")
    else:
        outfile = open(sys.argv[1].replace(".asm", ".hack"), "x")

    outfile.truncate(0)

    labelre = re.compile("^\((.*?)\)")
    # Parse all labels
    for line in infile:
        nocomment = line.split("//", 1)[0]
        stripped = nocomment.strip()
        if len(stripped) > 0:
            match = labelre.match(stripped);
            if match:
                label = match.group(1)
                lut[label] = currentCmd
            else:
                currentCmd += 1

    infile.seek(0)
    # Translate everything else
    for line in infile:
        nocomment = line.split("//", 1)[0]
        stripped = nocomment.strip()
        if len(stripped) > 0:
            match = labelre.match(stripped)
            if not match:
                process_line(stripped)

    infile.close()
    outfile.close()

def getbin(num, n=0):
    return format(num, 'b').zfill(n)

def getcomp(comp):
    cmdluta = {
        "0": "101010",
        "1": "111111",
        "-1": "111010",
        "D": "001100",
        "A": "110000",
        "!D": "001101",
        "!A": "110001",
        "-D": "001111",
        "-A": "110011",
        "D+1": "011111",
        "A+1": "110111",
        "D-1": "001110",
        "A-1": "110010",
        "D+A": "000010",
        "D-A": "010011",
        "A-D": "000111",
        "D&A": "000000",
        "D|A": "010101",
    }

    cmd = ""
    if comp.find("M") != -1:
        cmd += "1"
    else:
        cmd += "0"

    compa = comp.replace("M", "A")

    cmd += cmdluta[compa]

    return cmd

def getdest(dest):
    cmd = ""

    if dest.find("A") != -1:
            cmd += "1"
    else:
        cmd += "0"

    if dest.find("D") != -1:
            cmd += "1"
    else:
        cmd += "0"

    if dest.find("M") != -1:
            cmd += "1"
    else:
        cmd += "0"
    return cmd

def getjmp(jmp):
    return {
        "JGT": "001",
        "JEQ": "010",
        "JGE": "011",
        "JLT": "100",
        "JNE": "101",
        "JLE": "110",
        "JMP": "111",
    }[jmp]


def process_line(line):
    global lut
    global currentAddr
    global currentCmd
    global outfile
    global infile

    if line[0] == "@":
        addr = line[1:]
        if not addr.isdigit():
            if not addr in lut:
                lut[addr] = currentAddr
                currentAddr += 1
            outfile.write("0" + getbin(lut[addr], 15) +"\n")
        else:
            outfile.write("0" + getbin(int(addr), 15) + "\n")
    else:
        if "=" in line:
            dest=line.split("=",1)[0]
            comp = line.split("=", 1)[1]
        else:
            dest = False

        if ";" in line:
            split = line.split(";")
            jmp = split[len(split) -  1]
            comp = line.split(";", 1)[0]
        else:
            jmp = False

        if not comp:
            comp = line

        cmd = "111"

        cmd += getcomp(comp)

        if dest:
            cmd += getdest(dest)
        else:
            cmd += "000"

        if jmp:
            cmd += getjmp(jmp)
        else:
            cmd += "000"

        
        outfile.write(cmd + "\n")

if __name__ == '__main__':
    main()
