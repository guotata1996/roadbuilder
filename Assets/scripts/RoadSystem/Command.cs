using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Command
{
    void Execute(object data);
    void Undo();
}
