#include <exception>
#include <experimental/filesystem>
#include <iostream>
#include <string>
#include <vector>

#include "Parser.h"
#include "Writer.h"

Parser *parser;
Writer *writer;

int main(int argc, char **argv) {
    if (argc != 2) {
        std::cout << "Not enough arguments" << std::endl;
        return 1;
    }

    std::string inpath(argv[1]);
    bool shouldWriteInit = false;
    std::string indir;
    std::vector<std::string> infiles;
    std::string outfile;

    if (std::experimental::filesystem::is_directory(inpath)) {
        if (inpath[inpath.length()] == '/') {
            inpath = inpath.substr(0, inpath.length() - 1);
        }
        int slashPos = inpath.rfind("/");
        indir = inpath.substr(slashPos + 1, inpath.length());
        outfile = inpath + "/" + indir + ".asm";

        for (auto &f :
             std::experimental::filesystem::directory_iterator(inpath)) {
            if (std::experimental::filesystem::is_regular_file(
                    f.path().string())) {
                if (f.path().extension() == ".vm") {
                    infiles.push_back(f.path().string());
                }
            }
        }
        shouldWriteInit = true;
    } else if (std::experimental::filesystem::is_regular_file(inpath)) {
        int dotPos = inpath.rfind(".");
        outfile = inpath.substr(0, dotPos);
        outfile += ".asm";

        infiles.push_back(inpath);
    } else {
        std::cout << "Input file doesn't exist" << std::endl;
    }

    try {
        writer = new Writer(outfile);
    } catch (const std::invalid_argument &e) {
        std::cout << e.what() << std::endl;
        return 3;
    }

    if (shouldWriteInit) {
        writer->writeInit();
    }

    for (auto &f : infiles) {
        parser = new Parser(f);
        writer->setFile(f);
        while (parser->more()) {
            Command cmd;
            cmd = parser->next();
            writer->writeCmd(cmd);
        }
        parser->close();
        delete parser;
    }

    writer->close();

    return 0;
}
