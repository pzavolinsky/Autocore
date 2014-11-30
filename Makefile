.PHONY: all test coverage show package push-package clean samples update-version bump
all: coverage

# === Config ========================================================= #

MONO:=../mono-custom
NUGET:=../NuGet.exe

ASSEMBLIES:=Autocore/bin/Debug/Autocore.dll Autocore.Test/bin/Debug/Autocore.Test.dll
RELEASE:=Autocore/bin/Release/Autocore.dll

# === Build ========================================================== #
SOURCES:=$(shell find Autocore -type f -not -path '*/bin/*' -not -path '*/obj/*' -not -name '*.nuspec')
$(ASSEMBLIES):
	mdtool build

$(RELEASE): $(SOURCES) update-version
	@echo "\n===> Building Release binaries\n"
	@cd Autocore; mdtool build -c:Release

# === Test =========================================================== #
test: test-results/coverage.cov
coverage: test-results/coverage/project.html
test-results/coverage.cov: $(ASSEMBLIES)
	@echo "\n===> Running tests\n"
	@mkdir -p test-results/coverage
	@. $(MONO)/mono-env
	@LD_LIBRARY_PATH=$(MONO)/lib mono --debug --profile=monocov:outfile=test-results/coverage.cov,+[Autocore] $(MONO)/lib/mono/4.5/nunit-console.exe Autocore.Test/bin/Debug/Autocore.Test.dll
	@mv TestResult.xml test-results

test-results/coverage/project.html: test-results/coverage.cov
	@echo "\n===> Computing code coverage\n"
	@MONO_PATH=Autocore.Test/bin/Debug $(MONO)/bin/monocov --export-html=test-results/coverage test-results/coverage.cov
	@xdg-open $@

show:
	xdg-open test-results/coverage/project.html

# === Version ======================================================== #
UPD_FILE:=                                                     \
	diff $$file.new $$file > /dev/null;                        \
	if [ $$? -ne 0 ]; then                                     \
		echo "  - $$file updated";                             \
		mv $$file.new $$file;                                  \
	else                                                       \
		rm $$file.new;                                         \
	fi

update-version: Autocore.sln
	@VER=`cat Autocore.sln | sed -ne 's/.*version = \([0-9.]*\).*/\1/p'`; \
	echo "Solution version: $$VER";                            \
	for file in `find -name AssemblyInfo.cs`; do               \
		sed -e "s/^\[assembly: AssemblyVersion.*/[assembly: AssemblyVersion(\"$$VER.*\")]/" \
		    -e "s/^\[assembly: AssemblyFileVersion.*/[assembly: AssemblyFileVersion(\"$$VER.0\")]/" \
		$$file > $$file.new; \
		$(UPD_FILE);                                           \
	done;                                                      \
	for file in `find -name '*.nuspec'`; do                    \
		sed -e "s/<version>.*<\/version>/<version>$$VER<\/version>/" $$file > $$file.new; \
		$(UPD_FILE);                                           \
	done;                                                      \
	for file in `find -name '*.csproj'`; do                    \
		sed -e "s/<ReleaseVersion>.*<\/ReleaseVersion>/<ReleaseVersion>$$VER<\/ReleaseVersion>/" $$file > $$file.new; \
		$(UPD_FILE);                                           \
	done

bump:
	@VER=`cat Autocore.sln | sed -ne 's/.*version = \([0-9.]*\).*/\1/p'`; \
	echo "Current version: $$VER";                             \
	echo "New version: ${VERSION}";                            \
	file=Autocore.sln;                                         \
	sed -e "s/version = \([0-9.]*\)/version = ${VERSION}/" $$file > $$file.new; \
	$(UPD_FILE)

# === Package ======================================================== #
VERSION=$(shell cat Autocore/Autocore.nuspec | sed -ne 's/.*<version>\(.*\)<\/version>.*/\1/p')
PACKAGE=pkg/Autocore.$(VERSION).nupkg

package: $(PACKAGE)
$(PACKAGE): Autocore/Autocore.nuspec $(RELEASE)
	@echo "\n===> Packaging binaries\n"
	@mkdir -p pkg
	@cd pkg; ../$(MONO)/bin/mono ../$(NUGET) pack ../Autocore/Autocore.nuspec -Verbosity detailed

push-package: $(PACKAGE)
	XDG_CONFIG_HOME=~/.mono $(MONO)/bin/mono $(NUGET) push $< -Verbosity detailed

# === Clean ========================================================== #
BINARIES:=$(shell find \( -name 'bin' -o -name 'obj' \) -a -type d)
clean:
	@if [ ! -z "$(BINARIES)" ]; then \
		echo Will remove: ;\
		echo -n '  - ' ;\
		echo $(BINARIES) | sed -e 's/ /\n  - /g' ; \
		rm -rI $(BINARIES); \
	fi
	rm -f $(PACKAGE)
	rm -rf test-results

# === Samples ======================================================== #
samples:
	. $(MONO)/mono-env; \
	cd Samples; \
	for sample in $$(find * -type d -prune); do \
		echo "--> Running: $$sample"; \
		LD_LIBRARY_PATH=$(MONO)/lib mono $$sample/bin/Debug/$$sample.exe; \
		echo; \
	done
