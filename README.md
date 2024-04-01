# OwlCML160 MQTT

A command-line utility designed to run permanently to publish realtime measurements received by Owl Energy Meter CM160 on serial port to HomeAssistant. This can be used in the Energy dashboard to monitor house-hold consumption.

| Device | HA |
|---|---| 
| ![image](https://github.com/PBrunot/OwlCML160_MQTT/assets/6236243/f117920f-d593-40c4-bf29-3f8721d3feac) | ![image](https://github.com/PBrunot/OwlCML160_MQTT/assets/6236243/e702aef5-1f9c-42b0-85b0-17b1696e8cbf) |


## Installation

* Install Owl software (we need the serial driver) and check the serial port in the Device Manager.
  
* Compile & run the utility

```
OwlCM160_MQTT.exe -p COM5 -u <user> -p <password> -b <broker_ip>
```

* Expected output

```
MQTT and Serial connections established, starting main loop.
01/04/2024 12:55:40 : received realtime data: Current : 2,870 A, Power : 0,660 kW
01/04/2024 12:56:04 : received realtime data: Current : 2,800 A, Power : 0,644 kW
```

Note: it takes a while upon connection before live measurements arrive (minutes), because the CM160 is designed to send first all historical measurements, then provide live data.

## Homeassistant configuration

* Add a sensor for current measurement in configuration.yaml

```yaml
mqtt:
  sensor:
    - name: "OWL - Current"
      state_topic: "homeassistant/mains/current"
      unit_of_measurement: "A"
```

* Add a sensor for power estimation (facultative). Please note this value is calculated by multiplying the current with a fixed value (configurable in command line, default 230V).

```yaml
mqtt:
  sensor:
    - name: "OWL - Power"
      state_topic: "homeassistant/mains/power"
      unit_of_measurement: "kW"
```

* If you have access to a voltage measurement (<code>average_voltage</code>), you can also create a template to calculate a more accurate power multiplying the current measurement with voltage measurement.

```
{{ states('sensor.owl_current')  | float * states('sensor.average_voltage') | float / 1000 }}
```

* Add an energy counter as Helper > Riemann integration in kWh from the power sensor or template above.

## Command line reference

* Options available:
  
```
  -v, --verbose     Set output to verbose messages.

  -s, --serial      Required. Serial port for communication with CM160.

  -b, --broker      (Default: homeassistant.local) MQTT broker name or IP Address

  -t, --topic       (Default: homeassistant/mains/current) MQTT Topic to publish the realtime current

  --topic2          (Default: homeassistant/mains/power) MQTT Topic to publish the realtime power, facultative.

  -u, --user        (Default: ) Username to log on the MQTT broker

  -p, --password    (Default: ) Password to log on the MQTT broker

  --volts           (Default: 230) Average voltage (used for Power publication, P=U.I)

  --help            Display this help screen.

  --version         Display version information.
```
