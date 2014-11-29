.PHONY: all test coverage show package push-package clean samples
all: coverage

# === Config ========================================================= #

MONO:=../mono-custom
NUGET:=../NuGet.exe

ASSEMBLIES:=Autocore/bin/Debug/Autocore.dll Autocore.Test/bin/Debug/Autocore.Test.dll
RELEASE:=Autocore/bin/Release/Autocore.dll
BINARIES:=$(shell find \( -name 'bin' -o -name 'obj' \) -a -type d)
VERSION:=$(shell cat Autocore/Autocore.nuspec | sed -ne 's/.*<version>\(.*\)<\/version>.*/\1/p')
PACKAGE:=pkg/Autocore.$(VERSION).nupkg

# === Build ========================================================== #
$(ASSEMBLIES):
	mdtool build

$(RELEASE):
	cd Autocore; mdtool build -c:Release

# === Test =========================================================== #
test: test-results/coverage.cov
coverage: test-results/coverage/project.html
test-results/coverage.cov: $(ASSEMBLIES)
	@echo "=== Running tests ============="
	mkdir -p test-results/coverage
	. $(MONO)/mono-env
	LD_LIBRARY_PATH=$(MONO)/lib mono --debug --profile=monocov:outfile=test-results/coverage.cov,+[Autocore] $(MONO)/lib/mono/4.5/nunit-console.exe Autocore.Test/bin/Debug/Autocore.Test.dll
	mv TestResult.xml test-results

test-results/coverage/project.html: test-results/coverage.cov
	@echo "=== Computing code coverage ==="
	MONO_PATH=Autocore.Test/bin/Debug $(MONO)/bin/monocov --export-html=test-results/coverage test-results/coverage.cov
	xdg-open $@

show:
	xdg-open test-results/coverage/project.html

# === Package ======================================================== #
package: $(PACKAGE)
$(PACKAGE): $(RELEASE)
	mkdir -p pkg
	cd pkg; ../$(MONO)/bin/mono ../$(NUGET) pack ../Autocore/Autocore.nuspec -Verbosity detailed

push-package: $(PACKAGE)
	XDG_CONFIG_HOME=~/.mono $(MONO)/bin/mono $(NUGET) push $< -Verbosity detailed

# === Clean ========================================================== #
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
