CC = mcs
BIN = tomcatmanager.exe
SRC = \
Tomcatmanager.cs \
FileManager.cs

LIB = /reference:System.Xml.Linq

build:
	@echo "+----------------------------------------------+"
	@echo "| Compiling $(BIN)............"
	@echo "+----------------------------------------------+"
	$(CC) -out:$(BIN) $(SRC) $(LIB)
	@echo "+----------------------------------------------+"
	@echo "| Writing permissions on $(BIN)"
	@echo "+----------------------------------------------+"
	@chmod 764 $(BIN)
