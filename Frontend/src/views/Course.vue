<template>
  <div>
    <div>
      <a
        class="backButton"
        @click="goBack"
      > {{ getString("Back") }} </a>
    </div>
    <div v-if="validCourse">
      <div class="title">
        <span class="course-name">{{ getLang(getCourseName) }}</span>
      </div>
      <Dropdown 
        v-if="isMobile"
        v-model="selectedDay"
        :options="days" 
        optionLabel="day" 
        placeholder=""
        style="width: 100%;"
        scrollHeight="300px"
        :change="(e) => {index = e.value.index;}"
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
        :hide-weekends="false"
        :time-from="8 * 60"
        :time-to="22 * 60"
        :time-step="30"
        :selectedDate="isMobile ? (new Date()).addDays(selectedDay.index - ((new Date).getDay() == 0 ? 7: (new Date).getDay())) : (new Date())"
        hide-week-numbers
        hide-view-selector
        hide-title-bar
        :disable-views="['years', 'year', 'month', isMobile ? 'week' : 'day']"
        :events="getCourseEvents"
        :watch-real-time="false"
        :locale="getLanguage"
        :on-event-click="showGroup"
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
            <div class="event-place">
              {{ getLang(event.content) }}
            </div>
          </div>
          <span> {{ view ? "" : "" }} </span>
        </template>
      </vue-cal>
    </div>
    <Error404 v-else />
  </div>
</template>

<script>
import VueCal from "vue-cal";
import Error404 from "../components/Error404";
import router from "../router/index";
import Dropdown from "primevue/dropdown";
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import "vue-cal/dist/i18n/pl.js";

export default {
  name: "Course",
  components: { VueCal, Error404, Dropdown },
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
  computed: mapGetters([
    "getLanguage",
    "getCourseEvents",
    "validCourse",
    "getCourseName",
    "getString",
    "getLang"
  ]),
  created() {
    this.fetchCourseEvents(this.$route.params.id);
    this.isMobile = screen.width <= 620;
    this.isMedium = screen.width <= 920;
    window.addEventListener("resize", () => {
      this.isMobile = screen.width <= 620;
      this.isMedium = screen.width <= 920;
    });
  },
  methods: {
    ...mapActions(["fetchCourseEvents", "clearCourseData"]),
    goBack() {
      this.clearCourseData();
      router.push({ name: "Timetable" });
    },
    showGroup(event, e) {
      e.stopPropagation();
      this.clearCourseData();
      router.push(`/course_group/${event.id}`);
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

<style scoped>
.backButton {
  width: 100%;
  background-color: rgb(0, 0, 205);
  border: 1px solid darkblue;
  color: white !important;
  padding: 10px 24px;
  cursor: pointer;
  float: left;
}

.course-name {
  width: 100%;
  background-color: rgb(30, 144, 255);
  border: 1px solid darkblue;
  color: white;
  padding: 10px 24px;
  float: left;
}

.title::after {
  content: "";
  clear: both;
  display: table;
}
</style>
