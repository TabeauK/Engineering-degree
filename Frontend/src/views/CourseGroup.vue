<template>
  <div>
    <ConfirmDialog />
    <div>
      <a
        class="backButton"
        @click="goBack"
      > {{ getString("Go back") }} </a>
    </div>
    <div v-if="validGroup">
      <div class="table-wrapper">
        <table class="fl-table">
          <tbody>
            <tr>
              <td>{{ getString("Subject") }}</td>
              <td>{{ getLang(getGroupData.courseName) }}</td>
            </tr>
            <tr>
              <td>{{ getString("Group") }}</td>
              <td>{{ getLang(getGroupData.name) }}</td>
            </tr>
            <tr>
              <td>{{ getString("Class type") }}</td>
              <td>{{ getString(getGroupData.type) }}</td>
            </tr>
            <tr>
              <td>{{ getString("Lecturer") }}</td>
              <td>{{ getGroupData.lecturer }}</td>
            </tr>
            <tr>
              <td>{{ getString("Place") }}</td>
              <td>{{ getLang(getGroupData.place) }}</td>
            </tr>
            <tr>
              <td>{{ getString("Meeting times") }}</td>
              <td>
                <ScrollPanel
                  style="width: 100%; height: 200px;"
                  class="scrollpanel"
                >
                  <ul
                    v-for="date in getGroupData.dates"
                    :key="date"
                  >
                    {{
                      date
                    }}
                  </ul>
                </ScrollPanel>
              </td>
            </tr>
            <tr>
              <td>{{ getString("Max number of students") }}</td>
              <td>{{ getGroupData.placeLimit }}</td>
            </tr>
            <tr>
              <td>{{ getString("Current number of students") }}</td>
              <td>{{ getStudents.length }}</td>
            </tr>
            <tr v-if="getGroupData.irregular">
              <td>{{ getString("Irregular") }}</td>
              <td>{{ getLang(getGroupData.irregularity) }}</td>
            </tr>
          </tbody>
        </table>
      </div>
      <Button
        v-if="getStudnetStatus == 'CanJoin'"
        class="button"
        :label="getString('Join')"
        @click="join"
      />
      <Button
        v-else-if="getStudnetStatus == 'AlreadyJoining'"
        class="button"
        :label="getString('Stop joining')"
        @click="stopJoining"
      />
      <Button
        v-else
        class="button"
        disabled="disabled"
        :label="getString('Already in this group')"
      />
      <div class="student-table-wrapper">
        <DataTable
          paginatorTemplate="CurrentPageReport FirstPageLink PrevPageLink PageLinks NextPageLink LastPageLink RowsPerPageDropdown"
          :rowsPerPageOptions="[10,20,50]"
          :currentPageReportTemplate="getString('currentPageReportTemplate')"
          :alwaysShowPaginator="false"
          :value="getStudents"
          :paginator="true"
          :loading="getLoadingstudents"
          :rows="20"
        >
          <template #empty>
            {{ getString("No students found") }}
          </template>
          <template #loading>
            {{ getString("Loading students, please wait...") }}
          </template>
          <Column
            field="nr"
            header=""
            headerStyle="width: 10%"
          >
            <template #body="slotProps">
              <strong>{{ slotProps.data.nr }}</strong>
            </template>
          </Column>
          <Column
            field="name"
            :header="getString('Students')"
            headerStyle="width: 50%"
          >
            <template #body="slotProps">
              <div
                v-if="slotProps.data.userId == getId"
                class="me"
              >
                {{ slotProps.data.name + " (" + getString("you") + ")" }}
              </div>
              <div v-else>
                {{ slotProps.data.name }}
              </div>
            </template>
          </Column>
          <Column
            field="invite"
            headerStyle="width: 40%;"
          >
            <template #body="slotProps">
              <Button
                v-if="
                  slotProps.data.status == 'accepted' ||
                    slotProps.data.userId == getId"
                class="p-button-outlined p-button-success invite-btn"
                disabled="disabled"
                style="float: right;"
                :label="getString('Already in your group')"
              />
              <Button
                v-else-if="slotProps.data.status == 'invited'"
                class="p-button-outlined invite-btn"
                style="float: right;"
                :label="getString('Stop inviting')"
                @click="
                  loading = true;
                  stopInviting(slotProps.data);"
              />
              <Button
                v-else
                class="invite-btn"
                style="float: right;"
                :label="getString('Invite user')"
                @click="
                  loading = true;
                  invite(slotProps.data.userId).then((loading = false));"
              />
            </template>
          </Column>
        </DataTable>
      </div>
    </div>
    <Error404 v-else />
  </div>
</template>

<script>
import router from "../router/index";
import Error404 from "../components/Error404";
import DataTable from "primevue/datatable";
import Column from "primevue/column";
import Button from "primevue/button";
import ConfirmDialog from "primevue/confirmdialog";
import { mapGetters } from "vuex";
import { mapActions } from "vuex";
import ScrollPanel from "primevue/scrollpanel";

export default {
  name: "CourseGroup",
  components: {
    Error404,
    DataTable,
    Column,
    Button,
    ConfirmDialog,
    ScrollPanel
  },
  computed: mapGetters([
    "validGroup",
    "getGroupData",
    "getCourseId",
    "getStudents",
    "getStudnetStatus",
    "getId",
    "getLoadingstudents",
    "getString",
    "getLang"
  ]),
  created() {
    this.fetchCourseGroup(this.$route.params.id);
  },
  methods: {
    ...mapActions([
      "fetchCourseGroup",
      "join",
      "stopJoining",
      "invite",
      "stopInviting"
    ]),
    goBack() {
      if (!this.validGroup) {
        router.push({ name: "Timetable" });
      } else {
        router.push(`/course/${this.getCourseId}`);
      }
    }
  }
};
</script>

<style scoped>
.backBtn {
  position: absolute;
  left: 50px;
  top: 100px;
}

.backButton {
  width: 100%;
  background-color: rgb(0, 0, 205);
  border: 1px solid darkblue;
  color: white !important;
  padding: 10px 24px;
  cursor: pointer;
  float: left;
}

.table-wrapper {
  margin: 0 calc(50% - 304px) 30px;
  box-shadow: 0 35px 50px rgba(0, 0, 0, 0.2);
}

.student-table-wrapper {
  margin: 0 calc(50% - 404px) 30px;
  box-shadow: 0 35px 50px rgba(0, 0, 0, 0.2);
}

.fl-table {
  width: 100%;
  table-layout: fixed;
}

.fl-table tr {
  padding: 20px 20px 20px 20px;
  height: 75px;
  box-sizing: border-box;
  width: 300px;
  text-align: left;
}

.fl-table td {
  min-height: 75px;
  padding: 1em;
  font-weight: 500;
}

.fl-table tr:nth-child(even) > td:nth-child(odd) {
  color: #fff;
  background: #324960;
  width: 200px;
}

.fl-table tr:nth-child(even) > td:nth-child(even) {
  background: #f8f8f8;
  background: transparent;
}

.fl-table tr:nth-child(odd) > td:nth-child(odd) {
  color: #fff;
  background: rgb(30, 144, 255);
  width: 200px;
}

.fl-table tr:nth-child(odd) > td:nth-child(even) {
  background: #f8f8f8;
  border-right: 1px solid #e6e4e4;
}

.button {
  font-size: 24px;
  max-width: 600px;
  width: 80%;
  margin: 0 0 30px;
  align-items: center;
  background: rgb(30, 144, 255);
  box-shadow: 0 35px 50px rgba(0, 0, 0, 0.2);
  border-radius: 40px;
  height: 60px;
  padding: 0 30px;
  color: #fff;
  transition: background 0.3s, transform 0.3s;
  outline: 0;
  border: none;
}

.button:hover {
  background: rgb(0, 114, 255);
  transform: translate3d(0, -2px, 0);
}

.button:active {
  border-radius: 40px;
}

div.me {
  color: blueviolet;
}

.invite-btn {
  width: 130px;
  height: 60px;
}

@media only screen and (max-width: 630px) {
  .table-wrapper {
    margin: 0 0 30px;
  }

  .student-table-wrapper {
    margin: 0 0 30px;
  }

  .p-scrollpanel-content > ul {
    padding-left: 0%;
  }
}
</style>

<style>
button.invite-btn {
  font-size: 12px;
  width: calc(100% + 24px);
  margin: -12px;
  align-items: center;
  background: rgb(30, 144, 255);
  height: 50px;
  color: #fff;
  transition: background 0.3s;
  border: none;
  position: relative;
}

button.invite-btn:hover {
  background: rgb(0, 114, 255);
}

.card {
  padding-top: 0 !important;
  padding-bottom: 0 !important;
}

.p-scrollpanel-wrapper {
  border-right: 4px solid #f4f4f4;
}

.p-scrollpanel-bar {
  background-color: rgb(0, 114, 255) !important;
  opacity: 1 !important;
  transition: background-color 0.3s;
}

.p-scrollpanel-bar:hover {
  background-color: #135ba1 !important;
}
</style>
