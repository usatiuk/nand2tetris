#ifndef PARSER_H
#define PARSER_H

#include <exception>
#include <fstream>
#include <iostream>
#include <sstream>
#include <string>

#include "Commands.h"

class Parser {
   private:
    std::ifstream infile;

   public:
    Parser(std::string file);
    void close();
    bool more();
    Command next();
};

#endif  // PARSER_H
