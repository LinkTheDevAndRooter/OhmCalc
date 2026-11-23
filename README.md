# OhmCalc v0.1

Calculadora eléctrica portátil en C# (Windows Forms) compilada como ejecutable independiente sin necesidad de instalador.

## Funciones

* **Ley de Ohm:** cálculo de Voltaje ($V$), Corriente ($I$) y Resistencia ($R$).
* **Potencia:** estimación/disipación de potencia ($P = V \cdot I$) asociada a los cálculos.
* **Resistencia de conductor:** cálculo a partir de resistividad, longitud y área.
* **Bandas de resistencia:** soporte para **4 y 5 bandas** con cálculo de valor, tolerancia, rango permitido y vista previa visual.

## Compilación con `csc.exe`

Abre la consola (`cmd`) en la carpeta del proyecto y ejecuta:

```cmd
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /r:System.Windows.Forms.dll,System.Drawing.dll /out:OhmCalc.exe OhmCalc.cs
