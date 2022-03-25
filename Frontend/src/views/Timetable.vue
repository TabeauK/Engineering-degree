<template>
  <div>
    <div class="btn-group">
      <ToggleButton
        v-model="showEvents"
        :on-label="getString('My schedule')"
        :off-label="getString('My schedule')"
        on-icon="pi pi-check"
        off-icon="pi pi-times"
        class="toggle"
      />
      <ToggleButton
        v-model="showUsosEvents"
        :on-label="getString('Schedule from Usos')"
        :off-label="getString('Schedule from Usos')"
        on-icon="pi pi-check"
        off-icon="pi pi-times"
        class="toggle"
      />
      <ToggleButton
        v-if="!isMobile"
        v-model="showWeekends"
        :on-label="getString('Weekends')"
        :off-label="getString('Weekends')"
        on-icon="pi pi-check"
        off-icon="pi pi-times"
        class="toggle"
      />
    </div>
    <Dropdown 
      v-if="isMobile"
      v-model="selectedDay"
      :options="days" 
      optionLabel="day" 
      placeholder=""
      style="width: 100%;"
      scrollHeight="300px"
    >
      <template #option="slotProps">
        <div class="p-dropdown-car-option">
          <span>{{ getString(slotProps.option.day) }}</span>
        </div>
      </template>
      <template #value="slotProps">
        <div class="p-dropdown-car-option">
          <span>{{ getString(slotProps.value.day) }}</span>
        </div>
      </template>
    </Dropdown>
    <vue-cal
      :activeView="isMobile ? 'day' : 'week'"
      :hide-weekends="!getShowWeekends"
      :time-from="8 * 60"
      :time-to="22 * 60"
      :time-step="30"
      :selectedDate="isMobile ? (new Date()).addDays(selectedDay.index - ((new Date).getDay() == 0 ? 7: (new Date).getDay())) : (new Date())"
      hide-week-numbers
      hide-view-selector
      hide-title-bar
      :disable-views="['years', 'year', 'month', isMobile ? 'week' : 'day']"
      :events="getEvents.filter(x => !x.class.includes('null')).concat(getUsosEvents)"
      :watch-real-time="true"
      :locale="getLanguage"
      :on-event-click="showCourse"
    >
      <template #cell-content="{}" />
      <template #event="{ event, view }">
        <div class="vuecal__event-info">
          <div
            v-if="!isMedium"
            class="vuecal__event-time" 
          >
            {{
              event.start.formatTime("HH:mm") +
                " - " +
                event.end.formatTime("HH:mm")
            }}
          </div>
          <div class="vuecal__event-class">
            {{ getString(event.classType).substring(0,3).toUpperCase() }}
          </div>
        </div>
        <div 
          v-if="!isMedium || isMobile"
          class="vuecal__event-title"
        >
          {{ getLang(event.title) }}
        </div>
        <div
          v-else
          class="vuecal__event-title"
        >
          {{ makeShorter(getLang(event.title)) }}
        </div>
        <div class="vuecal__event-lecturer">
          {{ event.lecturer }}
        </div>
        <div
          v-if="!isMedium"
          class="vuecal__event-content"
        >
          {{ getLang(event.content) }}
        </div>
        <span> {{ view ? "" : "" }} </span>
      </template>
    </vue-cal>
  </div>
</template>

<script>
import VueCal from "vue-cal";
import router from "../router/index";
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import ToggleButton from "primevue/togglebutton";
import Dropdown from "primevue/dropdown";
import "vue-cal/dist/i18n/pl.js";

export default {
  name: "Timetable",
  components: { VueCal, ToggleButton, Dropdown },
  data() {
    return {
      isMedium: screen.width <= 920,
      isMobile: screen.width <= 620,
      days: [
        {day: "Monday", index: 1},
        {day: "Tuesday", index: 2},
        {day: "Wednesday", index: 3},
        {day: "Thursday", index: 4},
        {day: "Friday", index: 5},
        {day: "Saturday", index: 6},
        {day: "Sunday", index: 7},
      ],
      selectedDay: {day: "Monday", index: 1},
      index: 1,
    };
  },
  computed: {
    ...mapGetters([
      "getLanguage",
      "getEvents",
      "getUsosEvents",
      "getShowWeekends",
      "getShowEvents",
      "getShowUsosEvents",
      "getStartWeek",
      "getLang",
      "getString"
    ]),
    showEvents: {
      get() {
        return this.getShowEvents;
      },
      set(value) {
        this.toggleShowEvents(value);
      }
    },
    showUsosEvents: {
      get() {
        return this.getShowUsosEvents;
      },
      set(value) {
        this.toggleShowUsosEvents(value);
      }
    },
    showWeekends: {
      get() {
        return this.getShowWeekends;
      },
      set(value) {
        this.toggleShowWeekends(value);
      }
    }
  },
  created() {
    this.fetchUserInfo();
    this.fetchEvents();
    this.isMobile = screen.width <= 620;
    this.isMedium = screen.width <= 920;
    window.addEventListener("resize", () => {
      this.isMobile = screen.width <= 620;
      this.isMedium = screen.width <= 920;
    });
  },
  methods: {
    ...mapActions([
      "toggleShowUsosEvents",
      "toggleShowEvents",
      "toggleShowWeekends",
      "fetchEvents",
      "fetchUserInfo",
      "clearEvents"
    ]),
    showCourse(event, e) {
      e.stopPropagation();
      this.clearEvents();
      router.push(`/course/${event.id}`);
    },
    makeShorter(text) {
      var out = "";
      text.split(" ").forEach(element => {
        out += element[0];
      });
      return out.toUpperCase();
    }
  }
};
</script>

<style>
.vuecal__event.irregular {
  background:
    repeating-linear-gradient(
      45deg,
      transparent,
      transparent 5px,
      gray 5px,
      gray 10px,
    );
}

.p-carousel-items-content {
  display: flex;
  justify-content: center;
  align-items: center;
}

.vuecal__event.cwi {
  background-color: #644885;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.pro {
  background-color: #136d75;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.wyk {
  background-color: #273562;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.lab {
  background-color: #bc3369;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.cwi.Submitted {
  background:
    repeating-linear-gradient(
      45deg,
      transparent,
      transparent 10px,
      #412f57 10px,
      #412f57 20px
    );
  background-color: #644885;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.pro.Submitted {
  background:
    repeating-linear-gradient(
      45deg,
      transparent,
      transparent 10px,
      #0d4c52 10px,
      #0d4c52 20px
    );
  background-color: #136d75;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.wyk.Submitted {
  background:
    repeating-linear-gradient(
      45deg,
      transparent,
      transparent 10px,
      #18213d 10px,
      #18213d 20px
    );
  background-color: #273562;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.lab.Submitted {
    background:
    repeating-linear-gradient(
      45deg,
      transparent,
      transparent 10px,
      #87264c 10px,
      #87264c 20px
    );
  background-color: #bc3369;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.Rejected {
  background: linear-gradient(
    30deg,
    #808080 45%,
    #696969 45%,
    #696969 55%,
    #808080 55%) no-repeat;
  /* Start #5cbcb0 from 0 and end at 5%, Start #fff at 5% and end at 15%, Start #5cbcb0 again at 15% and end at 100% */
  background-repeat: no-repeat; /* To avoid multiple instances */
  background-color: gray;
  border: 1px solid black;
  color: seashell;
}

.vuecal__event.Rejected::after {
  position: absolute;
  top: 0;
  bottom: 0;
  left: 0;
  right: 0;
  content: "\d7"; /* use the hex value here... */
  font-size: 200px;
  color: #CCCCCC !important;
  line-height: 100px;
  text-align: center;
}

.vuecal__event {
  cursor: pointer;
}

.vuecal__event-info {
  position: relative;
  height: 20px;
}

.vuecal__event-title {
  font-size: 1rem;
  font-weight: bold;
}

.vuecal__event-content,
.vuecal__event-class,
.vuecal__event-time {
  color: lightgrey;
}

.vuecal__event > div {
  padding: 0px;
}

.vuecal__event-time,
.vuecal__event-class {
  position: absolute;
  width: auto;
  padding: 0px;
}

.vuecal__event-class {
  text-align: right;
  right: 0;
}

.vuecal__event-time {
  text-align: left;
  left: 0;
}

.weekday-label > span {
  display: none;
}

.weekday-label > span.full {
  display: block;
}

.vuecal__menu,
.vuecal__cell-events-count,
.vuecal__title-bar {
  background-color: rgb(30, 144, 255);
}

/* end global */
.btn-group {
  width: 100%;
}

.toggle {
  width: calc(100% / 3);
  background-color: rgb(0, 0, 205);
  border: 1px solid darkblue;
  color: white;
  padding: 10px 24px;
  cursor: pointer;
  float: left;
}

@media only screen and (max-width: 620px) {
  .toggle {
    width: calc(100% / 2);
  }
}

.btn-group::after {
  content: "";
  clear: both;
  display: table;
}

.btn-group a.toggled {
  background-color: rgb(30, 144, 255);
}

.btn-group a:hover {
  background-color: blue;
}
</style>
