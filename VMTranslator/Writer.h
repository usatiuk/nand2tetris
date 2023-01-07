#ifndef WRITER_H
#define WRITER_H

#include <exception>
#include <fstream>
#include <iostream>
#include <string>
#include <experimental/filesystem>

#include "Commands.h"
class Writer {
   private:
    std::ofstream outfile;
    void writeArith(Command cmd);
    void writePushPop(Command cmd);
    void writeBranch(Command cmd);
    void writeCall(Command cmd);
    void writeFunction(Command cmd);
    void writeReturn(Command cmd);

    std::string getFileName(std::string path);
    std::string getClassName(std::string filename);
    std::string filename;
    std::string classname;

    std::string functionName;
    int retCounter = 0;

   public:
    Writer(std::string file);
    void setFile(std::string filename);
    void close();
    void writeCmd(Command cmd);
    void writeInit();
};

#endif  // WRITER_H
