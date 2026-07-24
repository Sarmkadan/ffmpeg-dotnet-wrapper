#!/usr/bin/env python3
"""
Utility script used by the Aider toolchain to execute common .NET commands.

The script is intentionally lightweight – it avoids pulling in any external
dependencies and works with the standard library only.  It supports a few
convenient shortcuts:

    python3 src/Utilities/aider_buildcmd.py test   # runs `dotnet test`
    python3 src/Utilities/aider_buildcmd.py build  # runs `dotnet build`
    python3 src/Utilities/aider_buildcmd.py clean  # runs `dotnet clean`
    python3 src/Utilities/aider_buildcmd.py <any other command>

If an unknown shortcut is supplied, the argument is passed verbatim to
`subprocess.run` so you can execute arbitrary shell commands as needed.
"""

import sys
import subprocess
from pathlib import Path

def _run(command: str) -> int:
    """
    Execute a shell command, printing it first for visibility.

    Returns the exit code from ``subprocess.run``.
    """
    print(f"Running command: {command}")
    # ``shell=True`` allows the command string to be interpreted by the shell,
    # which is useful for complex commands (e.g., with pipes).  We deliberately
    # do not capture stdout/stderr here – they are streamed directly to the
    # console, matching the typical developer experience.
    result = subprocess.run(command, shell=True)
    return result.returncode

def _print_usage() -> None:
    script_name = Path(sys.argv[0]).name
    print(f"""Usage: python3 {script_name} <command>

Supported shortcuts:
  test   – Run `dotnet test` for the solution.
  build  – Run `dotnet build` for the solution.
  clean  – Run `dotnet clean` for the solution.

Any other argument is passed directly to the shell."""
    )

def main() -> int:
    if len(sys.argv) < 2:
        _print_usage()
        return 1

    cmd = sys.argv[1].lower()

    # Map known shortcuts to the appropriate dotnet commands.
    if cmd == "test":
        return _run("dotnet test")
    if cmd == "build":
        return _run("dotnet build")
    if cmd == "clean":
        return _run("dotnet clean")

    # Fallback: treat the first argument as a raw command string.
    # Preserve any additional arguments exactly as the user supplied them.
    raw_cmd = " ".join(sys.argv[1:])
    return _run(raw_cmd)

if __name__ == "__main__":
    sys.exit(main())
