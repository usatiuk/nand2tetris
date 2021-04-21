#include "Parser.h"

Parser::Parser(std::string file) {
    infile.open(file);
    if (!infile.is_open()) {
        throw std::invalid_argument("Input file doesn't exist");
    }
}

void Parser::close() { infile.close(); }

bool Parser::more() { return infile.good(); }

Command Parser::next() {
    std::string line;
    std::getline(infile, line);

    while ((line.substr(0, 2) == "//" || line.empty() ||
            (line[line.length() - 1] == '\r' && line.length() <= 1)) &&
           infile.good()) {
        std::getline(infile, line);
    }

    if (line.length() > 1 && line[line.length() - 1] == '\r') {
        line = line.substr(0, line.length() - 1);
    }

    if (line.length() == 0) {
        return Command(CommandType::Empty);
    }

    std::istringstream ss(line);
    std::string word;
    Command cmd;
    int n = 0;

    while (ss >> word) {
        if(word == "//") {
            break;
        }
        if (n == 0) {
            cmd.type = CommandTypeDict[word];
        } else if (n == 1) {
            cmd.arg1 = word;
        } else if (n == 2) {
            cmd.arg2 = std::stoi(word);
        } else {
            break;
        }
        n++;
    }

    return cmd;
}
