import utime as time
import log

log.basicConfig(level=log.INFO)
Time = log.getLogger("Time")

class TEST_TIME:
    def __init__(self, test_times=10,
                 test_secs=163461567,
                 test_tuple=(2021,10,21,9,23,0,3,296),
                 test_sleep_times=5):
        '''
        test_times：测试次数
        test_secs：转换的秒数
        test_tuple：转换的时间元组
        test_sleep_times：sleep，sleep_ms内部循环次数
        '''
        self.test_times = test_times
        self.test_secs = test_secs
        self.test_tuple = test_tuple
        self.test_sleep_times = test_sleep_times
    
    def RunTest(self):
        for i in range(self.test_times):
            Time.info('------test count %d------' % (i+1))
            if self.__TEST_localtime():
                Time.info('test api [Time.localtime([secs])] ------ pass')
            else:
                Time.info('test api [Time.localtime([secs])] ------ fail')
            if self.__TEST_mktime():
                Time.info('test api [Time.mktime(data)] ------ pass')
            else:
                Time.info('test api [Time.mktime(data)] ------ fail')
            if self.__TEST_sleep():
                Time.info('test api [Time.sleep(secs)] ------ pass')
            else:
                Time.info('test api [Time.sleep(secs)] ------ fail')
            if self.__TEST_sleep_ms():
                Time.info('test api [Time.sleep_ms(ms)] ------ pass')
            else:
                Time.info('test api [Time.sleep_ms(ms)] ------ fail')
            if self.__TEST_sleep_us():
                Time.info('test api [Time.sleep_us(us)] ------ pass')
            else:
                Time.info('test api [Time.sleep_us(us)] ------ fail')
            if self.__TEST_ticks_ms():
                Time.info('test api [Time.ticks_ms()] ------ pass')
            else:
                Time.info('test api [Time.ticks_ms()] ------ fail')
            if self.__TEST_ticks_us():
                Time.info('test api [Time.ticks_us()] ------ pass')
            else:
                Time.info('test api [Time.ticks_us()] ------ fail')
            if self.__TEST_time():
                Time.info('test api [Time.time()] ------ pass')
            else:
                Time.info('test api [Time.time()] ------ fail')

    def __TEST_localtime(self):
        try:
            Time.info("The tuple of time is ", time.localtime())
        except:
            return False
        try:
            Time.info("The tuple of time is ", time.localtime(self.test_secs))
        except:
            return False
        return True

    def __TEST_mktime(self):
        try:
            Time.info("The secs of time is ", time.mktime(self.test_tuple))
        except:
            return False
        return True

    def __TEST_sleep(self):
        try:
            for j in range(self.test_sleep_times):
                Time.info("<---Please compare the time!")
                time.sleep(1)
        except:
            return False
        return True

    def __TEST_sleep_ms(self):
        try:
            for j in range(self.test_sleep_times):
                Time.info("<---Please compare the time!")
                time.sleep_ms(1000)
        except:
            return False
        return True
        
    def __TEST_sleep_us(self):
        try:
            for j in range(self.test_sleep_times):
                Time.info("<---Please compare the time!")
                time.sleep_us(1000000)
        except:
            return False
        return True

    def __TEST_ticks_ms(self):
        try:
            for j in range(self.test_sleep_times):
                a=time.ticks_ms()
                time.sleep_ms(1000)
                Time.info("time.ticks_ms: result is ", time.ticks_diff(time.ticks_ms(), a))
        except:
            return False
        return True
        
    def __TEST_ticks_us(self):
        try:
            for j in range(self.test_sleep_times):
                a=time.ticks_us()
                time.sleep_us(1000000)
                Time.info("time.ticks_us: result is ", time.ticks_diff(time.ticks_us(), a))
        except:
            return False
        return True

    def __TEST_time(self):
        try:
            Time.info("time.time() is ", time.time())
        except:
            return False
        return True

if __name__ == '__main__':
    TEST_TIME().RunTest()