# Prueba de la Herramienta

La prueba de la herramienta se encuentra disponible en [Itch.io](https://elpatata2008.itch.io/farm-generator)

# Funcionamiento

El programa implementa un generador de terreno con granjas dividas en parcelas por vallas. Además de plantar árboles en los espacios donde no haya una valla, ni tampoco en parcelas donde no haya tierra arada. Además que tiene una variación al mismo donde las granjas ya fueron abandonadas, al punto de que está todo desolado, árboles muertos, vallas rotas, pasto muerto, entre otros.

## Generación
La generación sigue un orden "estricto" para lograr el resultado esperado:

### Terreno
El terreno es generado mediante el uso de Perlin Noise, un algoritmo para generar mapas de alturas. Conviene más usar este algoritmo ante otros, debido a que su generación es más "suave", es decir, no se generan ángulos bruscos, como rocas, sino que son más redondos como pequeñas colinas o cerros. Es importante generar este primero antes que el resto, ya que todo lo siguiente lo usa de base para hacer sus propias generaciones.

### Árboles: Cadena
> No es necesario generar este primero, pero si es necesario para después modelar los árboles. 

Empezando primero por el "código genético" del árbol, usa un L-System para calcular donde van las ramas, en qué rotación van, y qué tan largas son. Se les puede añadir más funciones, como hacer que en ciertos puntos genere una hoja. Lo que hace esto es iterar sobre la misma cadena de letras reemplazando cada una con su equivalente, es decir, si la cadena es "*FF*" y la regla es "*F = F+F*", entonces la primera iteración hará que cada "*F*" sea reemplazada por esa regla, dando como resultado "*F+FF+F*".

Con eso ya planteado, esa gramática se puede definir como a uno le plasca, no hace falta usar siempre la F para definir el largo de una rama, también se puede definir para decir el ángulo de la rama. Las reglas son libres a crear, pero hay un estándar, como usar la "*F*" para simular el paso (largo de la rama), "*+*" para rotar a la derecha y "*-*" a la izquierda.

### Parcelas
Lo siguiente a generar son las parecelas, las cuales son generadas mediante BSP (Partición Espacial Binaria), donde se usa principalmente en la generación de mazmorras, pero como las parcelas comparten una "lógica" similar, también sirve para este contexto. Empieza primero haciendo un corte, o una subdivisión en el terreno, luego de eso, dependiendo de la profundidad máxima designada, repetirá el mismo paso en cada cuadrante creado, con ciertas restricciones:

- **Profundidad Máxima**: Cada vez que se hace un corte, la profundidad aumenta, entonces esto continuará dependiendo de la profundidad máxima designada. De todas formas, se puede asignar un chance de que se detenga, sin importar la profundidad, de esta forma se puede hacer más aleatoria la generación.
- **Tamaño mínimo de la parcela**: Si el ancho o largo de la parcela es superior al tamaño mínimo designado, es posible hacer un corte. En el caso de que ambos cumplan con esta regla, se escojera la dirección del corte aleatoriamente.


### Vallas
Luego de haberse generado las parcelas, los cortes son guardados para poder saber los bordes de las mismas. Con esto, se puede generar las vallas a lo largo de la recta dependiendo del espaciado designado. Luego de haber generado los postes en la recta de pilares, se conectarán 2 travesaños entre los espacios, generando por completo la valla. Este proceso se repite en relación a la cantidad de cortes generados anteriormente al momento de generar las parecelas.

### Árboles: Plantación
Con la cadena ya completa, cada paso es guardado en una lista, donde se guardan las posición de inicio y de fin de cada línea en el mundo, con eso se generan las ramas. Las hojas son sólo un punto en el mundo, que también es definido por las reglas que uno implemente. Luego de ya haber generado el árbol en base a la cadena generada, se posiciona aleatoriamente en el terreno, con ciertas restricciones:

- **Distancia entre otros árboles**: 2 o más árboles no pueden estar a menos de una distancia designada.
- **Inclinación**: El árbol no se plantará en un punto del terreno donde la inclinación sea mayor a la designada.
- **Parcela arada**: El árbol sólo puede ser plantado en parcelas donde no haya tierra arada.
- **Margen de las vallas**: El árbol no puede estar a menos de un metro de la valla.

> Cada intento de plantación fallido será repetido nuevamente hasta 50 veces para poder garantizar la plantación de las mayor cantidad de árboles. Si después de los 50 intentos, no se logró encontrar un lugar válido, este será descartado y continuará con el siguiente árbol.

### Visuales
Debido a que la herramienta cuenta con un "switch" que permite cambiar de contexto, hay una configuración específica diseñada para este, donde se cambia de color la Skybox, la textura del pasto y la tierra arada, iluminación, y además se añade niebla.

Cuando se activa el modo, además de lo visual, cambia también el cómo se generan los árboles, ya que estos carecerán de hojas, las vallas se generarán de forma imcompleta, es decir, faltarán trozos de las mismas dando la ilusión de que está rota, y los árboles ya no tendrán la restricción de no poder ser plantados en parcelas con tierra arada, pudiendo ser plantados donde sea, excepto en los cortes de las mismas.

Al momento de generar con este modo activado, los cambios se harán respectivamente en los pasos correspondientes y no es una función que se aplica después de la generación, sino durante.
