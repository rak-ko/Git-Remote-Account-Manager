#include <iostream>
#include <stdio.h>
#include <string.h> 

int main(int argc, char *argv[])
{
    const std::string helpString = "-c [username] [email] Create new account\n"
    "-u Current use account\n"
    "-s [account id] Set current account\n"
    "-l List all accounts\n"
    "-i [file path] Import accounts from *.gam file\n"
    "-e [account id] Edit user\n"
    "-gamLoc Get .gam file location\n"
    "-h Help";
    
    //check for help
    std::string firstArg(argv[1]);
    if(argc > 1 && firstArg == "-h") { std::cout << helpString << std::endl; }

    system("pause");
    return 0;
}