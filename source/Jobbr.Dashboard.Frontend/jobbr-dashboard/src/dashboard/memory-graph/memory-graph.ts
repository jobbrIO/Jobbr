import { autoinject } from "aurelia-framework";
import { SmoothieChart, TimeSeries } from "smoothie";
import { ApiClient } from "resources/services/api-client";

@autoinject()
export class MemoryGraphCustomElement {

  private apiClient: ApiClient;

  private smoothie: SmoothieChart;

  constructor(private element: Element) {
    this.apiClient = new ApiClient();
  }

  attached() {
    this.apiClient.getMemoryInfo().then(data => {
      this.smoothie = new SmoothieChart({
        grid: {
          strokeStyle: '#39434f',
          fillStyle: '#22252B',
          lineWidth: 1,
          millisPerLine: 250,
          verticalSections: 5,
        },
        labels: { fillStyle: '#ffc533' },
        interpolation: 'step',
        minValue: 0,
        maxValue: data.TotalPhysicalMemory,
        limitFPS: 60,
      });

      let canvas = <HTMLCanvasElement>document.getElementById("memory-canvas");

      this.smoothie.streamTo(canvas, 1000);

      var line = new TimeSeries();

      setInterval(() => {
        this.apiClient.getMemoryInfo().then(data => {
          line.append(new Date().getTime(), data.FreeMemory);
        });
      }, 1000);

      this.smoothie.addTimeSeries(line, {
        strokeStyle: 'rgb(38, 108, 179)',
        fillStyle: 'rgba(38, 108, 179, 0.1)',
        lineWidth: 1
      });
    });
  }

  detached() {
    this.smoothie.stop();
  }
}
