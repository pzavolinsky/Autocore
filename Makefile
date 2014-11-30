.PHONY: all coverage
all: coverage

# === Config ========================================================= #

MONO:=../mono-custom
NUGET:=../NuGet.exe

# === Test =========================================================== #
.PHONY: test show
test: test-results/coverage.cov
coverage: test-results/coverage/project.html

ASSEMBLIES:=Autocore/bin/Debug/Autocore.dll Autocore.Test/bin/Debug/Autocore.Test.dll
$(ASSEMBLIES):
	mdtool build

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
.PHONY: bump
VERSION_FILES:=$(shell find -name AssemblyInfo.cs) \
	$(shell find -name '*.nuspec') \
	$(shell find -name '*.csproj')
UPD_FILE:=                                                     \
	diff $$file.new $$file > /dev/null;                        \
	if [ $$? -ne 0 ]; then                                     \
		echo "  - $$file updated";                             \
		mv $$file.new $$file;                                  \
	else                                                       \
		rm $$file.new;                                         \
		touch $$file;                                          \
	fi
VERSION_SHELL:=cat Autocore.sln | sed -ne 's/.*version = \([0-9.]*\).*/\1/p'

$(VERSION_FILES): Autocore.sln
	@VER=`$(VERSION_SHELL)`;                                   \
	file=$@;                                                   \
	if `echo $$file | grep 'AssemblyInfo.cs' > /dev/null`; then \
		sed -e "s/^\[assembly: AssemblyVersion.*/[assembly: AssemblyVersion(\"$$VER.0\")]/" \
		    -e "s/^\[assembly: AssemblyFileVersion.*/[assembly: AssemblyFileVersion(\"$$VER.0\")]/" \
		    $$file > $$file.new;                               \
		$(UPD_FILE);                                           \
	fi;                                                        \
	if `echo $$file | grep '\.nuspec$$' > /dev/null`; then     \
		sed -e "s/<version>.*<\/version>/<version>$$VER<\/version>/" \
		    -e "s/\(<dependency id=\"Autocore\".* version=\"\)[^\"]*\(\".*\)/\1$$VER\2/" \
		    $$file > $$file.new;                               \
		$(UPD_FILE);                                           \
	fi;                                                        \
	if `echo $$file | grep '\.csproj$$' > /dev/null`; then     \
		sed -e "s/<ReleaseVersion>.*<\/ReleaseVersion>/<ReleaseVersion>$$VER<\/ReleaseVersion>/" $$file > $$file.new; \
		$(UPD_FILE);                                           \
	fi

bump:
	@VER=`$(VERSION_SHELL)`;                                   \
	echo "Current version: $$VER";                             \
	echo "New version: ${VERSION}";                            \
	file=Autocore.sln;                                         \
	sed -e "s/version = \([0-9.]*\)/version = ${VERSION}/" $$file > $$file.new; \
	$(UPD_FILE)

# === Release ======================================================== #
.PHONY: release

SOURCES:=$(shell find -type f -path './Autocore*' -not -path '*/bin/*' -not -path '*/obj/*' -not -name '*.nuspec')
RELEASE_FILES:=$(shell find -type d -path './Autocore*' -prune -printf "%f/bin/Release/%f.dll\n")
release: $(RELEASE_FILES)


$(RELEASE_FILES): $(SOURCES)
	@echo "\n===> Building Release binaries for $@\n"
	@mdtool build -c:Release -p:`echo $@ | sed -e 's|/bin/.*||'`

# === Package ======================================================== #
.PHONY: package push-package
PKG_SPECS:=$(shell find -name 'package.nuspec')
PKG_VER=$(shell $(VERSION_SHELL))
PKG_BIN=$(patsubst ./%/package.nuspec,pkg/%.$(PKG_VER).nupkg,$(PKG_SPECS))

package: $(PKG_BIN)

$(PKG_BIN) : pkg/%.$(PKG_VER).nupkg : %/package.nuspec $(RELEASE_FILES)
	@echo "\n===> Packaging binaries: $@ from $<\n"
	@mkdir -p pkg
	@cd pkg; ../$(MONO)/bin/mono ../$(NUGET) pack ../$< -Verbosity detailed

push-package: $(PKG_BIN)
	@for file in $^; do                                        \
		XDG_CONFIG_HOME=~/.mono $(MONO)/bin/mono $(NUGET) push $$file -Verbosity detailed; \
	done

# === Clean ========================================================== #
.PHONY: clean
CLEAN_BINARIES:=$(shell find \( -name 'bin' -o -name 'obj' \) -a -type d)
clean:
	@if [ ! -z "$(CLEAN_BINARIES)" ]; then \
		echo Will remove: ;\
		echo -n '  - ' ;\
		echo $(CLEAN_BINARIES) | sed -e 's/ /\n  - /g' ; \
		rm -rI $(CLEAN_BINARIES); \
	fi
	rm -f pkg/*
	rm -rf test-results

# === Samples ======================================================== #
.PHONY: samples
samples:
	. $(MONO)/mono-env; \
	cd Samples; \
	for sample in $$(find * -type d -prune); do \
		echo "--> Running: $$sample"; \
		LD_LIBRARY_PATH=$(MONO)/lib mono $$sample/bin/Debug/$$sample.exe; \
		echo; \
	done
