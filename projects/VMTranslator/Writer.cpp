#include "Writer.h"

Writer::Writer(std::string file) {
    filename = getFileName(file);
    classname = getClassName(filename);

    outfile.open(file, std::ios::out | std::ios::trunc);
    if (!outfile.is_open()) {
        throw std::invalid_argument("Can't open output file");
    }
}

std::string Writer::getFileName(std::string path) {
    int slashPos = path.rfind("/");
    return path.substr(slashPos + 1, path.length());
}

std::string Writer::getClassName(std::string filename) {
    int dotPos = filename.rfind(".");
    return filename.substr(0, dotPos);
}

void Writer::setFile(std::string path) {
    filename = getFileName(path);
    classname = getClassName(filename);
}

void Writer::close() { outfile.close(); }

void Writer::writeFunction(Command cmd) {
    outfile << "(" << cmd.arg1 << ")" << std::endl;
    for (int i = 0; i < cmd.arg2; i++) {
        writeCmd(Command(CommandType::Push, "constant", 0));
    }

    functionName = cmd.arg1;
    retCounter = 0;
}

void Writer::writeCall(Command cmd) {
    std::string retLabel = functionName + "$ret." + std::to_string(retCounter);
    // push retAdress
    outfile << "@" << retLabel << std::endl;
    outfile << "D=A" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "M=D" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=M+1" << std::endl;

    outfile << "@LCL" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "M=D" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=M+1" << std::endl;

    outfile << "@ARG" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "M=D" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=M+1" << std::endl;

    outfile << "@THIS" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "M=D" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=M+1" << std::endl;

    outfile << "@THAT" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "M=D" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=M+1" << std::endl;

    outfile << "@SP" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@" << 5 + cmd.arg2 << std::endl;
    outfile << "D=D-A" << std::endl;
    outfile << "@ARG" << std::endl;
    outfile << "M=D" << std::endl;

    outfile << "@SP" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@LCL" << std::endl;
    outfile << "M=D" << std::endl;

    outfile << "@" << cmd.arg1 << std::endl;
    outfile << "-1;JMP" << std::endl;

    outfile << "(" << retLabel << ")" << std::endl;
    retCounter++;
}

void Writer::writeReturn(Command cmd) {
    // endFrame(R13)=LCL
    outfile << "@LCL" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@R13" << std::endl;
    outfile << "M=D" << std::endl;

    // retAddr(R14)=*(endFrame-5)
    outfile << "@5" << std::endl;
    outfile << "A=D-A" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@R14" << std::endl;
    outfile << "M=D" << std::endl;

    //*ARG = pop()
    outfile << "@SP" << std::endl;
    outfile << "M=M-1" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@ARG" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "M=D" << std::endl;

    // SP = ARG+1
    outfile << "@ARG" << std::endl;
    outfile << "D=M+1" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=D" << std::endl;

    // that =*(endFrame-1)
    outfile << "@R13" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@1" << std::endl;
    outfile << "A=D-A" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@THAT" << std::endl;
    outfile << "M=D" << std::endl;

    // this=*(endFrame-2)
    outfile << "@R13" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@2" << std::endl;
    outfile << "A=D-A" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@THIS" << std::endl;
    outfile << "M=D" << std::endl;

    // ARG=*(endFrame-3)
    outfile << "@R13" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@3" << std::endl;
    outfile << "A=D-A" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@ARG" << std::endl;
    outfile << "M=D" << std::endl;

    // LCL=*(endFrame-4)
    outfile << "@R13" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@4" << std::endl;
    outfile << "A=D-A" << std::endl;
    outfile << "D=M" << std::endl;
    outfile << "@LCL" << std::endl;
    outfile << "M=D" << std::endl;

    outfile << "@R14" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "0;JMP" << std::endl;
}

void Writer::writeCmd(Command cmd) {
    outfile << "// " << cmd << std::endl;
    switch (cmd.type) {
        case CommandType::Empty:
            break;
        case CommandType::Add:
        case CommandType::Sub:
        case CommandType::Neg:
        case CommandType::Eq:
        case CommandType::Gt:
        case CommandType::Lt:
        case CommandType::And:
        case CommandType::Or:
        case CommandType::Not:
            writeArith(cmd);
            break;
        case CommandType::Push:
        case CommandType::Pop:
            writePushPop(cmd);
            break;
        case CommandType::IfGoto:
        case CommandType::Goto:
        case CommandType::Label:
            writeBranch(cmd);
            break;
        case CommandType::Function:
            writeFunction(cmd);
            break;
        case CommandType::Call:
            writeCall(cmd);
            break;
        case CommandType::Return:
            writeReturn(cmd);
            break;
    }
    return;
}

void Writer::writeBranch(Command cmd) {
    switch (cmd.type) {
        case CommandType::IfGoto:
            outfile << "@SP" << std::endl;
            outfile << "M=M-1" << std::endl;
            outfile << "A=M" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@" << functionName << "$" << cmd.arg1 << std::endl;
            outfile << "D;JNE" << std::endl;
            break;
        case CommandType::Goto:
            outfile << "@" << functionName << "$" << cmd.arg1 << std::endl;
            outfile << "-1;JMP" << std::endl;
            break;
        case CommandType::Label:
            outfile << "(" << functionName << "$" << cmd.arg1 << ")"
                    << std::endl;
            break;
    }
    return;
}

void Writer::writeInit() {
    outfile << "@256" << std::endl;
    outfile << "D=A" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=D" << std::endl;
    writeCmd(Command(CommandType::Call, "Sys.init", 0));
}

void Writer::writeArith(Command cmd) {
    static int arithCounter = 0;
    if (cmd.type != CommandType::Neg && cmd.type != CommandType::Not) {
        outfile << "@SP" << std::endl;
        outfile << "M=M-1" << std::endl;
        outfile << "A=M" << std::endl;
        outfile << "D=M" << std::endl;
        outfile << "M=0" << std::endl;
        outfile << "@R13" << std::endl;
        outfile << "M=D" << std::endl;
        outfile << "@SP" << std::endl;
        outfile << "M=M-1" << std::endl;
        outfile << "A=M" << std::endl;
        outfile << "D=M" << std::endl;
        outfile << "@R14" << std::endl;
        outfile << "M=D" << std::endl;
    }

    switch (cmd.type) {
        case CommandType::Add:
            outfile << "@R13" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "D=M+D" << std::endl;
            break;
        case CommandType::Sub:
            outfile << "@R13" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "D=M-D" << std::endl;
            break;
        case CommandType::Neg:
            outfile << "@SP" << std::endl;
            outfile << "M=M-1" << std::endl;
            outfile << "A=M" << std::endl;
            outfile << "D=-M" << std::endl;
            break;
        case CommandType::Not:
            outfile << "@SP" << std::endl;
            outfile << "M=M-1" << std::endl;
            outfile << "A=M" << std::endl;
            outfile << "D=!M" << std::endl;
            break;

        case CommandType::Eq:
            outfile << "@R13" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "D=M-D" << std::endl;
            outfile << "@T" << arithCounter << std::endl;
            outfile << "D;JEQ" << std::endl;
            outfile << "@END" << arithCounter << std::endl;
            outfile << "D=0;JMP" << std::endl;
            outfile << "(T" << arithCounter << ")" << std::endl;
            outfile << "D=-1" << std::endl;
            break;
        case CommandType::Gt:
            outfile << "@R13" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "D=M-D" << std::endl;
            outfile << "@T" << arithCounter << std::endl;
            outfile << "D;JGT" << std::endl;
            outfile << "@END" << arithCounter << std::endl;
            outfile << "D=0;JMP" << std::endl;
            outfile << "(T" << arithCounter << ")" << std::endl;
            outfile << "D=-1" << std::endl;
            break;
        case CommandType::Lt:
            outfile << "@R13" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "D=M-D" << std::endl;
            outfile << "@T" << arithCounter << std::endl;
            outfile << "D;JLT" << std::endl;
            outfile << "@END" << arithCounter << std::endl;
            outfile << "D=0;JMP" << std::endl;
            outfile << "(T" << arithCounter << ")" << std::endl;
            outfile << "D=-1" << std::endl;
            break;
        case CommandType::And:
            outfile << "@R13" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "D=M&D" << std::endl;
            break;
        case CommandType::Or:
            outfile << "@R13" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "D=M|D" << std::endl;
            break;
    }
    outfile << "(END" << arithCounter << ")" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "A=M" << std::endl;
    outfile << "M=D" << std::endl;
    outfile << "@SP" << std::endl;
    outfile << "M=M+1" << std::endl;
    arithCounter++;
    return;
}

void Writer::writePushPop(Command cmd) {
    switch (cmd.type) {
        case CommandType::Push:
            if (cmd.arg1 == "constant") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
            } else if (cmd.arg1 == "local") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@LCL" << std::endl;
                outfile << "A=M+D" << std::endl;
                outfile << "D=M" << std::endl;
            } else if (cmd.arg1 == "argument") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@ARG" << std::endl;
                outfile << "A=M+D" << std::endl;
                outfile << "D=M" << std::endl;
            } else if (cmd.arg1 == "this") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@THIS" << std::endl;
                outfile << "A=M+D" << std::endl;
                outfile << "D=M" << std::endl;
            } else if (cmd.arg1 == "that") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@THAT" << std::endl;
                outfile << "A=M+D" << std::endl;
                outfile << "D=M" << std::endl;
            } else if (cmd.arg1 == "static") {
                outfile << "@" << classname << "." << cmd.arg2 << std::endl;
                outfile << "D=M" << std::endl;
            } else if (cmd.arg1 == "temp") {
                outfile << "@R" << 5 + cmd.arg2 << std::endl;
                outfile << "D=M" << std::endl;
            } else if (cmd.arg1 == "pointer") {
                if (cmd.arg2 == 0) {
                    outfile << "@THIS" << std::endl;
                } else if (cmd.arg2 == 1) {
                    outfile << "@THAT" << std::endl;
                }
                outfile << "D=M" << std::endl;
            }

            outfile << "@SP" << std::endl;
            outfile << "A=M" << std::endl;
            outfile << "M=D" << std::endl;
            outfile << "@SP" << std::endl;
            outfile << "M=M+1" << std::endl;
            break;
        case CommandType::Pop:
            outfile << "@SP" << std::endl;
            outfile << "M=M-1" << std::endl;
            outfile << "A=M" << std::endl;
            outfile << "D=M" << std::endl;
            outfile << "@R14" << std::endl;
            outfile << "M=D" << std::endl;

            if (cmd.arg1 == "local") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@LCL" << std::endl;
                outfile << "D=M+D" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "M=D" << std::endl;
                outfile << "@R14" << std::endl;
                outfile << "D=M" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "A=M" << std::endl;
                outfile << "M=D" << std::endl;
            } else if (cmd.arg1 == "argument") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@ARG" << std::endl;
                outfile << "D=M+D" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "M=D" << std::endl;
                outfile << "@R14" << std::endl;
                outfile << "D=M" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "A=M" << std::endl;
                outfile << "M=D" << std::endl;
            } else if (cmd.arg1 == "this") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@THIS" << std::endl;
                outfile << "D=M+D" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "M=D" << std::endl;
                outfile << "@R14" << std::endl;
                outfile << "D=M" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "A=M" << std::endl;
                outfile << "M=D" << std::endl;
            } else if (cmd.arg1 == "that") {
                outfile << "@" << cmd.arg2 << std::endl;
                outfile << "D=A" << std::endl;
                outfile << "@THAT" << std::endl;
                outfile << "D=M+D" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "M=D" << std::endl;
                outfile << "@R14" << std::endl;
                outfile << "D=M" << std::endl;
                outfile << "@R15" << std::endl;
                outfile << "A=M" << std::endl;
                outfile << "M=D" << std::endl;
            } else if (cmd.arg1 == "static") {
                outfile << "@" << classname << "." << cmd.arg2 << std::endl;
                outfile << "M=D" << std::endl;
            } else if (cmd.arg1 == "temp") {
                outfile << "@R" << 5 + cmd.arg2 << std::endl;
                outfile << "M=D" << std::endl;
            } else if (cmd.arg1 == "pointer") {
                if (cmd.arg2 == 0) {
                    outfile << "@THIS" << std::endl;
                } else if (cmd.arg2 == 1) {
                    outfile << "@THAT" << std::endl;
                }
                outfile << "M=D" << std::endl;
            }
            break;

            break;
    }
    return;
}
