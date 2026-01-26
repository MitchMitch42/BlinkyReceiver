/*
 * MIT License
 *
 * Copyright (c) 2024 Joey Castillo
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

#include <stdlib.h>
#include <string.h>
#include "light_sensor_face.h"
#include "tc.h"
#include "eic.h"
#include "usb.h"
#include "adc.h"

//#ifdef HAS_IR_SENSOR

typedef enum {
  BLINK_RECEIVER_MODE_IDLE,
  BLINK_RECEIVER_MODE_DISPLAY_CURRENT_LIGHT,
  BLINK_RECEIVER_MODE_WAIT_FOR_RISING_EDGE,
  BLINK_RECEIVER_MODE_RECORD
} blink_receiver_mode;

blink_receiver_mode light_sensor_mode;
uint8_t light_sensor_frequency = 8;
int light_sensor_border = 65440;
uint32_t light_sensor_data;
uint8_t light_sensor_bits_received;
uint16_t light_level;

void light_sensor_face_setup(uint8_t watch_face_index, void ** context_ptr) {
    (void) watch_face_index;
    (void) context_ptr;
}

void light_sensor_face_activate(void *context) {
    (void) context;
    HAL_GPIO_IR_ENABLE_out();
    HAL_GPIO_IR_ENABLE_clr();
    HAL_GPIO_IRSENSE_pmuxen(HAL_GPIO_PMUX_ADC);
    adc_init();
    adc_enable();
    movement_request_tick_frequency(light_sensor_frequency);
}

bool light_sensor_face_loop(movement_event_t event, void *context) {
    (void) context;

    switch (event.event_type) {
        case EVENT_NONE:
        case EVENT_ACTIVATE:
            light_sensor_mode = BLINK_RECEIVER_MODE_DISPLAY_CURRENT_LIGHT;
        case EVENT_TICK:
            switch(light_sensor_mode) {
                case BLINK_RECEIVER_MODE_DISPLAY_CURRENT_LIGHT: //normal behavior: show current light level                
                    light_level = adc_get_analog_value(HAL_GPIO_IRSENSE_pin());
                    char buf[7];
                    snprintf(buf, 7, "%-6d", light_level);
                    watch_display_text_with_fallback(WATCH_POSITION_TOP, "LIGHT", "LL");
                    watch_display_text(WATCH_POSITION_BOTTOM, buf);
                    break;
                case BLINK_RECEIVER_MODE_WAIT_FOR_RISING_EDGE: //wait for rising edge
                    // watch is polling sensor with increased frequency, waiting for a riging edge (dark -> bright)
                    light_level = adc_get_analog_value(HAL_GPIO_IRSENSE_pin());      
                    if (light_level < light_sensor_border) { //rising edge detected: transmission has started
                        //watch_buzzer_play_note(BUZZER_NOTE_E6, 100);  
                        light_sensor_bits_received = 0;
                        light_sensor_data = 0;
                        light_sensor_mode = BLINK_RECEIVER_MODE_RECORD;
                        movement_request_tick_frequency(light_sensor_frequency); //switch back to normal frequency for receiving the data
                    }
                    break;
                case BLINK_RECEIVER_MODE_RECORD:     
                        // read light sensor and save value (dark = 0, bright = 1) to corresponding bit in state->data
                        light_level= adc_get_analog_value(HAL_GPIO_IRSENSE_pin());
                        light_sensor_data = light_sensor_data << 1;
                        if (light_level < light_sensor_border) {         
                            light_sensor_data += 1;              
                        }
                        light_sensor_bits_received++;
                        if (light_sensor_bits_received == 32) { // transmission complete                 
                            //watch_buzzer_play_note(BUZZER_NOTE_E6, 100);
                            light_sensor_mode = BLINK_RECEIVER_MODE_IDLE;   
                            char outputString9[9];
                            snprintf(outputString9, 9, "%08x", light_sensor_data);
                            watch_display_string(outputString9, 2);
                        }
                    break;                      
                default: //IDLE
                    break;

            }           
            break;
        case EVENT_LIGHT_BUTTON_UP:
            // suppress LED, as it would interfere with light sensing
            break;
        case EVENT_ALARM_BUTTON_UP:
            // Start or interrupt listening to incoming transmission
            switch (light_sensor_mode) {
                case BLINK_RECEIVER_MODE_DISPLAY_CURRENT_LIGHT:
                case BLINK_RECEIVER_MODE_IDLE:
                    // start listening
                    //watch_buzzer_play_note(BUZZER_NOTE_E5, 100);
                    light_sensor_mode = BLINK_RECEIVER_MODE_WAIT_FOR_RISING_EDGE;
                    movement_request_tick_frequency(light_sensor_frequency * 2); //increase frequency to make sure that the first rising edge is detected as fast as possible
                    break;
                case BLINK_RECEIVER_MODE_WAIT_FOR_RISING_EDGE: 
                case BLINK_RECEIVER_MODE_RECORD:
                    // interrupt listening
                    //watch_buzzer_play_note(BUZZER_NOTE_E4, 200);
                    light_sensor_mode = BLINK_RECEIVER_MODE_IDLE;
                    movement_request_tick_frequency(light_sensor_frequency); //switch back to normal frequency
                    break;
                default:
                    break;
            }
            break;
            break;
        case EVENT_TIMEOUT:
            movement_move_to_face(0);
            break;
        default:
            return movement_default_loop_handler(event);
    }

    return false;
}

void light_sensor_face_resign(void *context) {
    (void) context;

    adc_disable();
    HAL_GPIO_IRSENSE_pmuxdis();
    HAL_GPIO_IRSENSE_off();
    HAL_GPIO_IR_ENABLE_off();
}

//#endif // HAS_IR_SENSOR
