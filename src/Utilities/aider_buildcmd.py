#!/usr/bin/env python3

import sys
import subprocess

def run_command(command):
    print(f"Running command: {command}")
    subprocess.run(command, shell=True)

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python3 aider_buildcmd.py <command>")
        sys.exit(1)

    command = sys.argv[1]
    run_command(command)
